// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU Lesser General Public License as published 
// by the Free Software Foundation; version 3 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License 
// for more details.
//
// You should have received a copy of the GNU Lesser General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Diagnostics;
using MariaDB.Data.MySqlClient.Properties;

namespace MariaDB.Data.MySqlClient
{
    internal class PerformanceMonitor
    {
        private MySqlConnection connection;
        private static PerformanceCounter procedureHardQueries;
        private static PerformanceCounter procedureSoftQueries;

        public PerformanceMonitor(MySqlConnection connection)
        {
            this.connection = connection;

            string categoryName = Resources.PerfMonCategoryName;

            if (connection.Settings.UsePerformanceMonitor && procedureHardQueries == null)
            {
                try
                {
                    procedureHardQueries = new PerformanceCounter(categoryName,
                                                                  "HardProcedureQueries", false);
                    procedureSoftQueries = new PerformanceCounter(categoryName,
                                                                  "SoftProcedureQueries", false);
                }
                catch (Exception ex)
                {
                    MySqlTrace.LogError(connection.ServerThread, ex.Message);
                }
            }
        }

#if DEBUG
        private void EnsurePerfCategoryExist()
        {
            CounterCreationDataCollection ccdc = new CounterCreationDataCollection();
            CounterCreationData ccd = new CounterCreationData();
            ccd.CounterType = PerformanceCounterType.NumberOfItems32;
            ccd.CounterName = "HardProcedureQueries";
            ccdc.Add(ccd);

            ccd = new CounterCreationData();
            ccd.CounterType = PerformanceCounterType.NumberOfItems32;
            ccd.CounterName = "SoftProcedureQueries";
            ccdc.Add(ccd);

            if (!PerformanceCounterCategory.Exists(Resources.PerfMonCategoryName))
                PerformanceCounterCategory.Create(Resources.PerfMonCategoryName, null, PerformanceCounterCategoryType.MultiInstance, ccdc);
        }
#endif

        public void AddHardProcedureQuery()
        {
            if (!connection.Settings.UsePerformanceMonitor ||
                procedureHardQueries == null)
                return;
            procedureHardQueries.Increment();
        }

        public void AddSoftProcedureQuery()
        {
            if (!connection.Settings.UsePerformanceMonitor ||
                procedureSoftQueries == null)
                return;
            procedureSoftQueries.Increment();
        }
    }
}