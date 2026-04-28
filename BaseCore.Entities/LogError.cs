
using BaseCore.Common;

namespace BaseCore.Entities
{
    public class LogError: Entity
    {
        public string Header { get; set; }
        public string Body { get; set; }
        public string Message { get; set; }
    }

}
