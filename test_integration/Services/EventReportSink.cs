using System.Threading.Tasks;


namespace test_integration.Services
{
    public interface IEventReportSink
    {
        TaskCompletionSource<string> Report { get; set;}
    }

    public class EventReportSink : IEventReportSink
    {
        public TaskCompletionSource<string> Report { get; set;}

        public EventReportSink()
        {
            Report = new TaskCompletionSource<string>();
        }
    }
}