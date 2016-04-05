namespace TicketingSystem.Server.Controllers
{
    using System;
    using System.Collections.Generic;

    using TicketingSystem.Data;

    public class ValuesController: BaseApiController
    {
        public ValuesController() : base(new TicketingSystemData())
        {
            
        }

        public ValuesController(ITicketingSystemData data)
            : base(data)
        {
        }

        public IEnumerable<string> Get()
        {
            throw new System.NotImplementedException();
        }

        public string Get(int v)
        {
            throw new NotImplementedException();
        }

        public void Post(string value)
        {
            throw new NotImplementedException();
        }

        public void Put(int i, string value)
        {
            throw new NotImplementedException();
        }

        public void Delete(int i)
        {
            throw new NotImplementedException();
        }
    }
}