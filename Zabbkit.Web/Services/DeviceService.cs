using System;
using System.Collections.Generic;
using System.Web.Http;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Zabbkit.Web.Models;
using log4net;
using System.Net;
using MongoDB.Bson;

namespace Zabbkit.Web.Services
{
    public class DeviceService : IDeviceService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DeviceService));
        private readonly MongoCollection<Device> _deviceCollection;

        public DeviceService(MongoCollection<Device> deviceCollection)
        {
            _deviceCollection = deviceCollection;
        }

        public void Create(Device device)
        {
            var byToken = Query.And(Query<Device>.EQ(e => e.Type, device.Type), Query<Device>.EQ(e => e.Token, device.Token));
            var res = _deviceCollection.FindOne(byToken);
            if (res == null)
            {
                device.Created = DateTime.UtcNow;
                _deviceCollection.Insert(device);
            }
            else
            {
                device.Id = res.Id;
            }
        }

        public void RenewTokent(TokenRenewRequest renewRequest)
        {
            var byId = Query<Device>.EQ(e => e.Id, renewRequest.Id);
            var byType = Query<Device>.EQ(e => e.Type, renewRequest.Type);
            var deviceLocator = Query.And(byType, byId);
            var device = _deviceCollection.FindOne(deviceLocator);
            if (device == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            if (device.Token != renewRequest.OldToken)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            //Ok, everuthing looks fine, let's renew it 
            var updateFields = new BsonDocument("Token", renewRequest.NewToken);
            var update = new UpdateDocument("$set", updateFields);
            _deviceCollection.Update(deviceLocator, update);
        }

        public void Delete(string id)
        {
            ObjectId.Parse(id);  //Validation
            var byId = Query<Device>.EQ(e => e.Id, id);
            var res = _deviceCollection.Remove(byId);
            if (res.DocumentsAffected == 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        public IEnumerable<Device> GetAll()
        {
            return _deviceCollection.FindAllAs<Device>();
        }

        public Device Get(string id)
        {
            var objId = ObjectId.Parse(id);
            return _deviceCollection.FindOneByIdAs<Device>(objId);
        }
    }
}