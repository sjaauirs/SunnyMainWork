namespace SunnyRewards.Helios.ETL.Infrastructure.RuleEngine
{
    public class Util
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public int ToInt(object val)
        {
            return Convert.ToInt32(val);
        }

        public string ToSuccess(params object[] vals)
        {
            string output = "";
            foreach (object val in vals)
            {
                output += string.Join(",", val.ToString());
            }
            return output;
        }
    }
}
