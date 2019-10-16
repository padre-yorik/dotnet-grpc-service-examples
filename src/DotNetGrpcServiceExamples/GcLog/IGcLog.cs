using System.IO;

namespace GcLog
{
    public interface IGcLog
    {
        void Start(string filename);
        void Start(StreamWriter writer);
        void Stop();
    }
}
