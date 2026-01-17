using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces
{
    public interface ICsvWrapper
    {
        /// <summary>
        /// Create a csv file in given local path
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="csvConfig"></param>
        /// <param name="records"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task CreateCsvFile<T>(CsvConfiguration csvConfig, List<T> records, string fileName) where T : class;
    }
}
