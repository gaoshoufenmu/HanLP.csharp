using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.seg
{
    public class SegConfig
    {
        /// <summary>
        /// 是否索引分词（合理地最小分词）
        /// </summary>
        public bool indexMode = false;
        /// <summary>
        /// 是否识别中国人名
        /// </summary>
        public bool chsNameRecognize = true;
        /// <summary>
        /// 是否识别音译人名
        /// </summary>
        public bool translatedNameRecognize = true;
        /// <summary>
        /// 是否识别日本人名
        /// </summary>
        public bool jpNameRecognize = false;
        /// <summary>
        /// 是否识别地名
        /// </summary>
        public bool placeRecognize = false;
        /// <summary>
        /// 是否识别机构名
        /// </summary>
        public bool orgRecognize = false;
        /// <summary>
        /// 是否加载用户词典
        /// </summary>
        public bool useCustomDict = true;
        /// <summary>
        /// 词性标注
        /// </summary>
        public bool natureTagging = false;
        /// <summary>
        /// 命名实体识别是否至少有一项被识别
        /// </summary>
        public bool nameEntityRecognize = true;
        /// <summary>
        /// 是否计算偏移量
        /// </summary>
        public bool offset = false;
        /// <summary>
        /// 是否识别数词和量词
        /// </summary>
        public bool numQuantRecognize = false;
        /// <summary>
        /// 并行分词线程数
        /// </summary>
        public int threadNum = 1;

        /// <summary>
        /// 更新命名实体识别
        /// </summary>
        public void UpdateNERConfig() => nameEntityRecognize = 
            chsNameRecognize || translatedNameRecognize || jpNameRecognize || placeRecognize || orgRecognize;
    }
}
