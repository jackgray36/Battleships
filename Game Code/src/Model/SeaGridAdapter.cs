﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace MyGame
{
    /// <summary>
    /// The SeaGridAdapter allows for the change in a sea grid view. Whenever a ship is
    /// presented it changes the view into a sea tile instead of a ship tile.
    /// </summary>
    public class SeaGridAdapter : ISeaGrid
    {
        private SeaGrid _MyGrid;

        /// <summary>
        /// Create the SeaGridAdapter, with the grid, and it will allow it to be changed.
        /// <param name="grid">the grid that needs to be adapted</param>
        /// </summary>
        public SeaGridAdapter(SeaGrid grid)
        {
            _MyGrid = grid;
            _MyGrid.Changed += new EventHandler(MyGrid_Changed);
        }

        /// <summary>
        /// MyGrid_Changed causes the grid to be redrawn by raising a changed event.
        /// <param name="sender">the object that caused the change</param>
        /// <param name="e">what needs to be redrawn</param>
        /// </summary>
        private void MyGrid_Changed(object sender, EventArgs e)
        {
            Changed?.Invoke(this, e);
        }

        /// <summary>
        /// Changes the discovery grid. Where there is a ship we will see water.
        /// <param name="x">tile x coordinate</param>
        /// <param name="y">tile y coordinate</param>
        /// <returns>a tile, either what it actually is, or if it was a ship then return a sea tile</returns>
        /// </summary>
        public TileView Item(int x, int y)
        {
            TileView result = _MyGrid.Item(x, y);

            if (result == TileView.Ship)
                return TileView.Sea;
            else
                return result;
        }

        /// <summary>
        /// Indicates that the grid has been changed.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Get the width of a tile.
        /// </summary>
        public int Width
        {
            get
            {
                return _MyGrid.Width;
            }
        }

        /// <summary>
        /// Get the height of the tile.
        /// </summary>
        public int Height
        {
            get
            {
                return _MyGrid.Height;
            }
        }

        /// <summary>
        /// HitTile calls opon _MyGrid to hit a tile at the row and column.
        /// <param name="row">the row its hitting at</param>
        /// <param name="col">the column its hitting at</param>
        /// <returns>The result from hitting that tile</returns>
        /// </summary>
        public AttackResult HitTile(int row, int col)
        {
            return _MyGrid.HitTile(row, col);
        }
    }
}