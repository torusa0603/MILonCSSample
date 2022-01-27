using System.Collections.Generic;

namespace Log
{
    public class CLogMain
    {
        #region 本クラス実体保持用Dictionary
        static Dictionary<string, COutputLog> m_dicLog = new Dictionary<string, COutputLog>();
		#endregion

		public static CDefine.UPDATE_TIME m_enUpdateTime = new CDefine.UPDATE_TIME();
		public static CDefine.DELETE_TIME m_enDeleteTime = new CDefine.DELETE_TIME();

		public CLogMain(string nstrKey, string nstrFileName, CDefine.UPDATE_TIME nenUpdateTime, CDefine.DELETE_TIME nenDeleteTime)
		{

			if (false == m_dicLog.ContainsKey(nstrKey))
			{
				m_dicLog.Add(nstrKey, new COutputLog(nstrFileName, nenUpdateTime, nenDeleteTime));
			}
		}

		/// <summary>
		/// Dictionaryからクラス実体の取得
		/// </summary>
		public static COutputLog getInstance(string nstrKey)
		{
			if (true == m_dicLog.ContainsKey(nstrKey))
			{
				return m_dicLog[nstrKey];
			}

			return null;
		}


	}
}
