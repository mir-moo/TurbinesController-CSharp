using System;
using System.Collections.Generic;

namespace TurbinesController
{
    public interface ITurbine
    {
        int turbineNumber { get; set; }
        string SendCommand(string cmd);

        //Task<string> SendCommandAsync(string cmd);
    }
    public class Turbine : ITurbine
    {
        public event EventHandler StatusChanged;
        protected virtual void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnStatusChanged();
            }
        }
        public int turbineNumber { get; set; }
        public Turbine(int turbineNumber)
        {
            this.turbineNumber = turbineNumber;
        }

        public string SendCommand(string cmd)
        {
            return $"The turbine {turbineNumber} get command: {cmd}";
        }

        #region async version
        //public async Task<string> SendCommandAsync(string cmd)
        //{
        //    await Task.Delay(1000); // Delay for 1 second to simulate I/O operation
        //    return $"The turbine {turbineNumber} get command: {cmd}";
        //}
        #endregion
    }

    public class CentralController
    {
        private readonly List<ITurbine> _turbines = new List<ITurbine>();

        public void AttachTurbine(ITurbine turbine)
        {
            if (!_turbines.Contains(turbine))
                _turbines.Add(turbine);
        }

        public string ShutdownTurbine(int turbineNumber)
        {
            var turbine = _turbines.Find(x => x.turbineNumber == turbineNumber);
            if (turbine == null)
                throw new Exception($" Turbine {turbineNumber} NOT Found!");

            _turbines.Remove(turbine);
            (turbine as Turbine).StatusChanged += Turbin_StatusChanged;
            return $"Turbine {turbineNumber} is OFF";
        }

        private void Turbin_StatusChanged(object sender, EventArgs e)
        {
            Console.WriteLine($"Turbine {(sender as Turbine).turbineNumber} received notification, new status: {(sender as Turbine).Status}");
        }

        #region async version
        //public async Task<List<string>> NotifyTurbineAsync(string cmd)
        //{
        //    var messages = new List<string>();

        //    foreach (ITurbine turbine in _turbines)
        //        messages.Add(await turbine.SendCommandAsync(cmd));

        //    return messages;
        //}
        #endregion

        public List<string> NotifyTurbine(string cmd)
        {
            var messages = new List<string>();

            foreach (ITurbine turbine in _turbines)
                messages.Add(turbine.SendCommand(cmd));

            return messages;

            #region Parrallel version
            //Parallel.ForEach(_turbines, turbine =>
            //{
            //    messages.Add(turbine.SendCommand(cmd));
            //});
            #endregion
        }
    }

    class Program
    {
        static void Main()
        {
            var controller = new CentralController();

            // The Central Controller attached some turbines
            controller.AttachTurbine(new Turbine(turbineNumber: 1));
            controller.AttachTurbine(new Turbine(turbineNumber: 2));
            controller.AttachTurbine(new Turbine(turbineNumber: 3));
            controller.AttachTurbine(new Turbine(turbineNumber: 4));

            // All turbines are on operating
            //var receivedMessages = await controller.NotifyTurbineAsync("[On Operation]");
            var receivedMessages = controller.NotifyTurbine("[On Operation]");
            foreach (var msg in receivedMessages)
                Console.WriteLine(msg);


            // The central controller shuts down several turbines
            Console.WriteLine(controller.ShutdownTurbine(2));
            Console.WriteLine(controller.ShutdownTurbine(4));


            // After shutting down some turbines 
            //var receivedMessages = await controller.NotifyTurbineAsync("[On Operation]");
            receivedMessages = controller.NotifyTurbine("[On Operation]");
            foreach (var msg in receivedMessages)
                Console.WriteLine(msg);

            Console.ReadKey();
        }
    }
}
