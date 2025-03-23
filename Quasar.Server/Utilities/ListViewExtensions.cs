using System.Windows.Forms;
using Quasar.Server.Controls;

namespace Quasar.Server.Utilities
{
    /// <summary>
    /// Provides extension methods for ListView/AeroListView controls.
    /// </summary>
    public static class ListViewExtensions
    {
        /// <summary>
        /// Stretches a column of a ListView to fill the remaining width.
        /// </summary>
        /// <param name="listView">The ListView to modify.</param>
        /// <param name="columnIndex">The index of the column to stretch.</param>
        public static void StretchColumnByIndex(this ListView listView, int columnIndex)
        {
            if (listView.Columns.Count == 0 || columnIndex < 0 || columnIndex >= listView.Columns.Count)
                return;

            int totalWidth = 0;
            for (int i = 0; i < listView.Columns.Count; i++)
            {
                if (i != columnIndex)
                    totalWidth += listView.Columns[i].Width;
            }

            int columnWidth = listView.ClientSize.Width - totalWidth;
            if (columnWidth > 0)
            {
                listView.Columns[columnIndex].Width = columnWidth;
            }
        }

        /// <summary>
        /// Determines if a column is stretched to fill the remaining width.
        /// </summary>
        /// <param name="listView">The ListView to check.</param>
        /// <param name="columnIndex">The index of the column to check.</param>
        /// <returns>True if the column is stretched, false otherwise.</returns>
        public static bool IsStretched(this ListView listView, int columnIndex)
        {
            if (listView.Columns.Count == 0 || columnIndex < 0 || columnIndex >= listView.Columns.Count)
                return false;

            int totalWidth = 0;
            for (int i = 0; i < listView.Columns.Count; i++)
            {
                if (i != columnIndex)
                    totalWidth += listView.Columns[i].Width;
            }

            int expectedWidth = listView.ClientSize.Width - totalWidth;
            return expectedWidth > 0 && listView.Columns[columnIndex].Width >= expectedWidth - 1 && 
                   listView.Columns[columnIndex].Width <= expectedWidth + 1;
        }
    }
} 