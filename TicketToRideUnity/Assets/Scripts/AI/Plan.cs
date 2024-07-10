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
	     * Diese Methode fügt Aktionen zur Liste der Aktionen hinzu. 
	     */
        public void AddActions(Action[] actions)
        {
            foreach (Action action in actions)
            {
                AddAction(action);
            }
        }

        /**
         * Diese Methode fügt eine Aktion dem Plan hinzu
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
         * Gibt Anzahl an Aktionen zurück
         */
        public int GetActionCount()
        {
            return actions.Count;
        }

        /**
         * Gibt Aktion an Stelle index im Plan zurück
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
        //        // So kann diese Aktion erkannt und der entsprechenden Liste hinzugefügt werden
        //        if (action.Contains("ActivateCityPlaces"))
        //        {
        //            this.actions.Add(new ActivateCityPlaces());
        //        }

        //        if (action.Contains("BuildCity"))
        //        {
        //            // Die KI war nicht wirklich in der Lage dazu, auszuwählen, wo eine Stadt, eine Siedlung oder eine Straße errichtet werden sollte.
        //            // Diese Schwierigkeit konnte dadurch gelöst werden, dass der Situationsbeschreibung Informationen zu den
        //            // Positionen alle Bauplätze hinzugefügt wurden, sodass die KI basierend auf diesen Informationen eine Auswahl treffen kann.

        //            // In dem String, der die durchzuführenden Aktionen enthält, sind Zeile und Spalte der Bauplätze durch einen
        //            // Doppelpunkt getrennt angegeben, und werden für alle drei Arten von Gebäuden auf gleiche Weise verarbeitet.  
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

        //        // Erweiterung Räuber setzen
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
