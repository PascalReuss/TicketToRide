using System;
using System.Runtime.Serialization;

namespace Assets.Scripts.AI
{
    [Serializable]
    public class Case
    {
        public string plan;
        public Situation situation;
        public string caseType;

        public Case()
        {

        }

        public Case(string plan, Situation situation, string caseType)
        {
            this.plan = plan;
            this.situation = situation;
            this.caseType = caseType;
        }
        
        public Case(string plan, string caseType)
        {
            this.plan = plan;
            this.caseType = caseType;
        }

        public Case(string plan, Situation situation)
        {
            this.plan = plan;
            this.situation = situation;
        }
    }
}
