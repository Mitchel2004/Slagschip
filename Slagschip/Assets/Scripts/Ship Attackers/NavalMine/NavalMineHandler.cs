using System.Collections.Generic;
using PlayerGrid;
using UIHandlers;
using UnityEngine;

namespace ShipAttackers.Mine
{
    using Data;
    public class NavalMineHandler : ShipAttackerHandler<NavalMineBehaviour>
    {
        [SerializeField] private DashboardHandler dashboardHandler;

        private List<GridCell> _mines = new();

        private void Awake()
        {
            GridHandler.instance.OnMineSet.AddListener(AddMine);
            dashboardHandler.onGameStart.AddListener(PlaceMines);
        }

        public void AddMine(GridCell _cell)
        {
            _mines.Add(_cell);
        }

        public void PlaceMines()
        {
            foreach (var mineCell in _mines)
            {
                AttackCell(mineCell);
            }
        }
    }
}
