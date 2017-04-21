using System;
using System.Collections.Generic;
using System.Net.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Zabbkit.Web.Models;
using log4net;

namespace Zabbkit.Web.Services
{
    public class TrackingService : ITrackingService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NotificationService));
        private readonly MongoCollection<TrackingRecord> _trackCollection;
        private readonly MongoCollection<Device> _deviceCollection;

        public TrackingService(MongoCollection<TrackingRecord> trackCollection, MongoCollection<Device> deviceCollection)
        {
            _trackCollection = trackCollection;
            _deviceCollection = deviceCollection;
        }

        public string Create(TrackingRecord record)
        {
            try
            {
                record.Created = DateTime.UtcNow;
                record.Updated = record.Created;
                _trackCollection.Insert(record);

                //Update device's last used
                var updateFields = new BsonDocument("LastUsed", record.Created);
                var update = new UpdateDocument("$set", updateFields);
                _deviceCollection.Update(Query<Device>.EQ(e => e.Id, record.DeviceId), update,
                                        WriteConcern.Unacknowledged);
            }
            catch (Exception ex)
            {
                Log.Error("Tracking failed", ex);
                return null;
            }
            return record.Id;
        }

        public void Update(TrackingStatus status, string trackingId, string message = null)
        {
            if (String.IsNullOrEmpty(trackingId))
            {
                Log.Warn("Unable to track message: empty trackingID");
                return;
            }

            try
            {
                var updateFields = new BsonDocument()
                    .Add("Updated", DateTime.UtcNow)
                    .Add("Status", status);
                if (message != null)
                    updateFields.Add("Description", message);
                var update = new UpdateDocument("$set", updateFields);
                _trackCollection.Update(Query<TrackingRecord>.EQ(e => e.Id, trackingId), update,
                                        WriteConcern.Unacknowledged);
            }
            catch (Exception ex)
            {
                Log.Error("Tracking failed", ex);
            }
        }

        public IGaTracker StartGaSession(HttpRequestMessage request, string userId)
        {
            return new GaTracker(request);
        }

        public TrackingRecord Get(string id)
        {
            var byId = Query<TrackingRecord>.EQ(e => e.Id, id);
            return _trackCollection.FindOneAs<TrackingRecord>(byId);
        }

        public IEnumerable<TrackingRecord> FindAll()
        {
            return _trackCollection.FindAllAs<TrackingRecord>();
        }
    }
}