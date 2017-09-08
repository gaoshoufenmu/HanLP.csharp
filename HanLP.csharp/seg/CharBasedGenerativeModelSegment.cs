/*
 * 统计概率学意义上的最佳分词建模方案有两种：1. Generative Model，采用最大联合概率 2. Discriminative Model，采用最大条件概率
 * 
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.seg
{
    /// <summary>
    /// 基于字构词的生成式模型分词器基类
    /// </summary>
    public abstract class CharBasedGenerativeModelSegment : Segment
    { }
}
