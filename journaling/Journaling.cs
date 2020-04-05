using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace Journaling
{
    public enum EventTypes
    {
        Create,
        Edit,
        Delete,
        Close
    }
    public enum EventStatus
    {
        Started,
        Doing,
        Done,

    }
    public class EventInfo
    {
        public String FileName;
        public EventTypes Type;
        public EventStatus Status;
        public String Addition;
        public String Uuid;
        public EventInfo(string fileName, EventTypes type, EventStatus status, string addition, string uuid="")
        {
            if (uuid == "")
            {
                Uuid = System.Guid.NewGuid().ToString();
            }
            else { Uuid = uuid; }
                FileName = fileName;
            Type = type;
            Status = status;
            Addition = addition;
            
        }
    };
    public class JournalStructure
    {
        public List<EventInfo> Journal;
        public JournalStructure()
        {
            Journal = new List<EventInfo>();
        }
        public void AddEvent(String FileName, EventTypes Type, EventStatus Status,String Addition="",String uuid="")
        {
            Journal.Add(new EventInfo(FileName, Type, Status,Addition,uuid));
        }
        public EventInfo GetLastEvent(string FileName)
        {
            return Journal[Journal.FindLastIndex(part => part.FileName == FileName)];
        }
        public void GetStartedEvents()
        {
            var Started = Journal.Select((Event) => Event.Uuid).ToList();
            var StartedMap = new HashSet<string>(Started);

            foreach (var StartedElement in StartedMap)
            {
                Started.Remove(StartedElement);
                if(!Started.Exists((val) => val == StartedElement))
                {
                    throw new Exception($"Found error with operation {StartedElement}");
                }
                
                
            }

        }
    }


}