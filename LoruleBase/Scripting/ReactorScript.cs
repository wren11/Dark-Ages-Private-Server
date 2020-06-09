using Darkages.Types;

namespace Darkages.Scripting
{
    public abstract class ReactorScript
    {
        public ReactorScript(Reactor reactor)
        {
            Reactor = reactor;
        }

        public Reactor Reactor { get; set; }

        public abstract void OnTriggered(Aisling aisling);
        public abstract void OnClose(Aisling aisling);
        public abstract void OnNext(Aisling aisling);
        public abstract void OnBack(Aisling aisling);
    }
}