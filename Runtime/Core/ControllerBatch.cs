namespace Atuvu.Lifecycle
{
    internal sealed class ControllerBatch
    {
        public string name { get; private set; }
        public IController[] controllers { get; private set; }

        public ControllerBatch(string name, IController[] controllers)
        {
            this.name = name;
            this.controllers = controllers;
        }
    }
}