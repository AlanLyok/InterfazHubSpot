using System.Xml;
using Mastersoft.Framework.Standard;

namespace Mastersoft.Scheduler452.Intefaces
{
    /// <summary>
    /// Shim interface — matches the contract expected by InterfazHubSpot.BatchProcess jobs.
    /// Replace with the real Mastersoft.Scheduler452.Intefaces.dll when available.
    /// </summary>
    public interface IScheduler
    {
        MSContext Contexto { get; set; }
        bool Finished { get; set; }
        void Execute(XmlElement oParam, XmlElement oReturn);
    }
}
