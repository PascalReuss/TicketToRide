using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Assets.Scripts.AI
{
    /**
     * Klasse, die den Plan eines KI-Spielers speichern kann.
     */
    [DataContract]
    public class Plan
    {
        /**
         * Liste zur Speicherung der Aktionen.
         */
        public List<Action> actions { get; set; }

        /**
	     * Der Plan als String 
	     */
        [DataMember]
        public string actionsAsString { get; set; }

        /**
         * Default-Konstruktor.
         */
        public Plan()
        {
            actions = new List<Action>();
        }

        /**
	     * Diese Methode f�gt Aktionen zur Liste der Aktionen hinzu. 
	     */
        public void AddActions(Action[] actions)
        {
            foreach (Action action in actions)
            {
                AddAction(action);
            }
        }

        /**
         * Diese Methode f�gt eine Aktion dem Plan hinzu
         */
        public void AddAction(Action action)
        {
            actions.Add(action);
        }

        /**
         * Diese Methode entfernt eine Aktion aus dem Plan
         */
        public void RemoveAction(Action action)
        {
            actions.Remove(action);
        }

        /**
         * Gibt Anzahl an Aktionen zur�ck
         */
        public int GetActionCount()
        {
            return actions.Count;
        }

        /**
         * Gibt Aktion an Stelle index im Plan zur�ck
         */
        public Action GetActionByIndex(int index)
        {
            return actions[index];
        }

        /**
         * Diese Methode erzeugt aus dem String eine Liste an Aktionen (Den Plan)
         */
        //public void StringToActions()
        //{
        //    this.actions = new List<Action>();
        //    if (actionsAsString == null)
        //    {
        //        actionsAsString = "EndTurn;";
        //    }
        //    string[] actions = actionsAsString.Split(';');
        //    foreach (string action in actions)
        //    {
        //        if (action.Contains("ActivateVillagePlaces"))
        //        {
        //            this.actions.Add(new ActivateVillagePlaces());
        //        }

        //        if (action.Contains("BuildVillage"))
        //        {
        //            string[] splits = action.Split(':');
        //            this.actions.Add(new BuildVillage(int.Parse(splits[1]), int.Parse(splits[2])));
        //        }

        //        // Erweiterung der Klasse, sodass das Spiel eine Anfrage zum Bau einer Stadt erkennen kann
        //        // So kann diese Aktion erkannt und der entsprechenden Liste hinzugef�gt werden
        //        if (action.Contains("ActivateCityPlaces"))
        //        {
        //            this.actions.Add(new ActivateCityPlaces());
        //        }

        //        if (action.Contains("BuildCity"))
        //        {
        //            // Die KI war nicht wirklich in der Lage dazu, auszuw�hlen, wo eine Stadt, eine Siedlung oder eine Stra�e errichtet werden sollte.
        //            // Diese Schwierigkeit konnte dadurch gel�st werden, dass der Situationsbeschreibung Informationen zu den
        //            // Positionen alle Baupl�tze hinzugef�gt wurden, sodass die KI basierend auf diesen Informationen eine Auswahl treffen kann.

        //            // In dem String, der die durchzuf�hrenden Aktionen enth�lt, sind Zeile und Spalte der Baupl�tze durch einen
        //            // Doppelpunkt getrennt angegeben, und werden f�r alle drei Arten von Geb�uden auf gleiche Weise verarbeitet.  
        //            string[] splits = action.Split(':');
        //            this.actions.Add(new BuildCity(int.Parse(splits[1]), int.Parse(splits[2])));
        //        }
        //        // Ende Stadt

        //        if (action.Contains("ActivateRoadPlaces"))
        //        {
        //            this.actions.Add(new ActivateRoadPlaces());
        //        }

        //        if (action.Contains("BuildRoad"))
        //        {
        //            string[] splits = action.Split(':');
        //            this.actions.Add(new BuildRoad(int.Parse(splits[1]), int.Parse(splits[2])));
        //        }

        //        // Erweiterung Entwicklungskarte kaufen
        //        if (action.Contains("BuyDevelopmentCard"))
        //        {
        //            //string[] splits = action.Split(':');
        //            this.actions.Add(new BuyDevelopmentCard());
        //        }

        //        // Erweiterung R�uber setzen
        //        //if (action.Contains("ActivateRoadPlaces"))
        //        //{
        //        //    this.actions.Add(new ActivateRoadPlaces());
        //        //}

        //        if (action.Contains("BuildThief"))
        //        {
        //            string[] splits = action.Split(':');
        //            //this.actions.Add(new BuildThief(int.Parse(splits[1]), int.Parse(splits[2])));
        //            this.actions.Add(new BuildThief(int.Parse(splits[1])));
        //        }

        //        if (action.Contains("EndTurn"))
        //        {
        //            this.actions.Add(new EndTurn());
        //        }
        //        if (action.Contains("RollDice"))
        //        {
        //            this.actions.Add(new RollDice());
        //        }
        //    }
        //}

        public override string ToString()
        {
            return actionsAsString + "ActionCount: " + GetActionCount();
        }
    }
}
