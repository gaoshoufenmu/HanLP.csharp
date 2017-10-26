using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp
{
    /// <summary>
    /// 全局配置
    /// </summary>
    public class Config
    {
        //! ---------------------------- 配置数据文件路径全部固定 ----------------------------
        /// <summary>
        /// 数据文件的根目录
        /// </summary>
        private static string DataRootDir = AppDomain.CurrentDomain.BaseDirectory + "/data/"; 
        /// <summary>
        /// 核心词典路径
        /// </summary>
        public static readonly string Core_Dict_Path = DataRootDir + "dictionary/CoreNatureDictionary.txt";
        /// <summary>
        /// 核心转移矩阵词典路径
        /// </summary>
        public static readonly string Core_TR_Dict_Path = DataRootDir + "dictionary/CoreNatureDictionary.tr.txt";
        /// <summary>
        /// 自定义词典路径
        /// </summary>
        public static string[] Custom_Dict_Path = new[]
        {
            DataRootDir + "dictionary/custom/CustomDictionary.txt"
        };
        /// <summary>
        /// 2元语法词典路径
        /// </summary>
        public static readonly string BiGram_Dict_Path = DataRootDir + "dictionary/CoreNatureDictionary.ngram.txt";
        /// <summary>
        /// 核心停用词词典
        /// </summary>
        public static readonly string Core_Stopword_Dict_Path = DataRootDir + "dictionary/stopwords.txt";
        /// <summary>
        /// 核心同义词词典路径
        /// </summary>
        public static readonly string Core_Synonym_Dict_Path = DataRootDir + "dictionary/synonym/CoreSynonym.txt";
        /// <summary>
        /// 人名词典路径
        /// </summary>
        public static readonly string Person_Dict_Path = DataRootDir + "dictionary/person/nr.txt";
        /// <summary>
        /// 人名转移矩阵词典路径
        /// </summary>
        public static readonly string Person_TR_Dict_Path = DataRootDir + "dictionary/person/nr.tr.txt";
        /// <summary>
        /// 地名词典路径
        /// </summary>
        public static readonly string Place_Dict_Path = DataRootDir + "dictionary/place/ns.txt";
        /// <summary>
        /// 地名转移矩阵词典路径
        /// </summary>
        public static readonly string Place_TR_Dict_Path = DataRootDir + "dictionary/place/ns.tr.txt";
        /// <summary>
        /// 机构名词典路径
        /// </summary>
        public static readonly string Org_Dict_Path = DataRootDir + "dictionary/organization/nt.txt";
        /// <summary>
        /// 机构名转移矩阵词典路径
        /// </summary>
        public static readonly string Org_TR_Dict_Path = DataRootDir + "dictionary/organization/nt.tr.txt";
        /// <summary>
        /// 简繁转换词典根目录
        /// </summary>
        public static readonly string Simple2Traditial_Dict_Dir = DataRootDir + "dictionary/tc/";
        /// <summary>
        /// 声母韵母语调词典路径
        /// </summary>
        public static readonly string SYT_Dict_Path = DataRootDir + "dictionary/pinyin/SYTDictionary.txt";
        /// <summary>
        /// 拼音词典路径
        /// </summary>
        public static readonly string Pinyin_Dict_Path = DataRootDir + "dictionary/pinyin/pinyin.txt";
        /// <summary>
        /// 音译人名词典路径
        /// </summary>
        public static readonly string Translated_Person_Dict_Path = DataRootDir + "dictionary/person/nrf.txt";
        /// <summary>
        /// 日本人名词典路径
        /// </summary>
        public static readonly string Japan_Person_Dict_Path = DataRootDir + "dictionary/person/nrj.txt";
        /// <summary>
        /// 字符类型表
        /// </summary>
        public static readonly string Char_Type_Path = DataRootDir + "dictionary/other/CharType.bin";
        /// <summary>
        /// 字符规则化表路径（全角2半角，繁体2简体）
        /// </summary>
        public static readonly string Char_Table_Path = DataRootDir + "dictionary/other/CharTable.txt";
        /// <summary>
        /// 词-词性-依存模型路径
        /// </summary>
        public static readonly string Word_Nature_Model_Path = DataRootDir + "model/dependency/WordNature.txt";
        /// <summary>
        /// 最大熵模型路径
        /// </summary>
        public static readonly string Max_Ent_Model_Path = DataRootDir + "model/dependency/MaxEntModel.txt";
        /// <summary>
        /// 神经网络分析模型路径
        /// </summary>
        public static readonly string NN_Parser_Model_Path = DataRootDir + "model/dependency/NNParserModel.txt";
        /// <summary>
        /// CRF分词模型路径
        /// </summary>
        public static readonly string CRF_Segment_Model_Path = DataRootDir + "model/segment/CRFSegmentModel.txt";
        /// <summary>
        /// HMM分词模型路径
        /// </summary>
        public static readonly string HMM_Segment_Model_Path = DataRootDir + "model/segment/HMMSegmentModel.bin";
        /// <summary>
        /// CRF依存模型路径
        /// </summary>
        public static readonly string CRF_Dependency_Model_Path = DataRootDir + "model/dependency/CRFDependencyModelMini.txt";

        /// <summary>
        /// 依存模型，词性保存路径
        /// </summary>
        public static readonly string Word_Nat_Weight_Model_Path = DataRootDir + "model/dependency/pos-thu.txt";

        /// <summary>
        /// 行政地名字典路径
        /// </summary>
        public static readonly string Area_Dict_Path = DataRootDir + "dictionary/place/area.txt";
        /// <summary>
        /// 公司名相关的高频词典
        /// </summary>
        public static readonly string Com_HighFreq_Dict_Path = DataRootDir + "dictionary/custom/ComHighFreq.txt";

        /// <summary>
        /// 是否显示词性
        /// </summary>
        public static bool ShowTermNature = true;
        /// <summary>
        /// 是否对字符正规化（繁体->简体，全角->半角）
        /// 切换此配置后必须删除CustomDictionary.txt.bin缓存
        /// </summary>
        public static bool NormalizeChar = false;


        //! ---------------------- 由于是作为dll使用，所以不提供debug模式 ---------------------------
    }
}
