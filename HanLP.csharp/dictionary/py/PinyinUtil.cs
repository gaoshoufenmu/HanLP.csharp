using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HanLP.csharp.dictionary.py
{
    public class PinyinUtil
    {
        private static Regex _pinyinWithToneNum = new Regex("[a-z]*[1-5]?", RegexOptions.Compiled);
        private const char _charA = 'a';
        private const char _charE = 'e';
        private const string _ouStr = "ou";
        private const string _allUnmarkedVowelStr = "aeiouv";
        private const string _allMarkedVowelStr = "āáǎàaēéěèeīíǐìiōóǒòoūúǔùuǖǘǚǜü"; // 5*5 矩阵依次排开

        /// <summary>
        /// 用数字表示音调的拼音转换为普通音标的拼音
        /// </summary>
        /// <param name="pinyin"></param>
        /// <returns></returns>
        public static string Pinyin_ToneNum2ToneMark(string pinyin)
        {
            pinyin = pinyin.ToLower();
            if(_pinyinWithToneNum.IsMatch(pinyin))
            {
                var unmarkedVowel = '$';
                var indexOfUnmarkedVowel = -1;

                var lastChar = pinyin[pinyin.Length - 1];
                if (lastChar >= '1' && lastChar <= '5')
                {
                    var toneNum = lastChar - '0';

                    int vowel_idx = pinyin.IndexOf(_charA);

                    if (vowel_idx != -1)
                    {
                        indexOfUnmarkedVowel = vowel_idx;
                        unmarkedVowel = _charA;
                    }
                    else
                    {
                        vowel_idx = pinyin.IndexOf(_charE);
                        if (vowel_idx != -1)
                        {
                            indexOfUnmarkedVowel = vowel_idx;
                            unmarkedVowel = _charE;
                        }
                        else
                        {
                            vowel_idx = pinyin.IndexOf(_ouStr);
                            if (vowel_idx != -1)
                            {
                                indexOfUnmarkedVowel = vowel_idx;
                                unmarkedVowel = _ouStr[0];
                            }
                            else
                            {
                                for (int i = pinyin.Length - 1; i >= 0; i--)
                                {
                                    if (_allUnmarkedVowelStr.Contains(pinyin[i]))
                                    {
                                        indexOfUnmarkedVowel = i;
                                        unmarkedVowel = pinyin[i];
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (unmarkedVowel != '$' && indexOfUnmarkedVowel != -1)
                    {
                        var rowIndex = _allUnmarkedVowelStr.IndexOf(unmarkedVowel);
                        var columnIndex = toneNum;

                        var vowelIndex = rowIndex * 5 + columnIndex;

                        char markedVowel = _allMarkedVowelStr[vowelIndex];

                        var sb = new StringBuilder(pinyin.Length);
                        sb.Append(pinyin.Substring(0, indexOfUnmarkedVowel).Replace('v', 'ü'));
                        sb.Append(markedVowel);
                        sb.Append(pinyin.Substring(indexOfUnmarkedVowel + 1).Replace('v', 'ü'));
                        return sb.ToString();
                    }

                    return pinyin;
                }
                // 没有音调的拼音
                return pinyin.Substring('v', 'ü');
            }
            return pinyin;
        }
    }
}
