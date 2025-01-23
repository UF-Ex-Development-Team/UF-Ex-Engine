namespace UndyneFight_Ex.Debugging
{
    internal partial class DebugWindow
    {
        internal partial class StringBlock
        {
            public void CheckType() => _type = TypeChecking(_data);
            /// <summary>
            /// 试着分裂一个不合法的块至多个块
            /// </summary>
            /// <returns>返回值是新的几个块</returns>
            public StringBlock[] Split()
            {
                List<string> strs = [];
                bool isSplitter = false;
                string current = "";
                int i;
                for (i = 0; i < _data.Length; i++)
                {
                    char currentChar = _data[i];
                    if (IsSplitter(currentChar))
                    {
                        if (!isSplitter)
                        {
                            if (current != "")
                                strs.Add(current);
                            isSplitter = true;
                            current = "";
                        }
                    }
                    else
                    {
                        if (isSplitter)
                        {
                            strs.Add(current);
                            isSplitter = false;
                            current = "";
                        }
                    }
                    current += currentChar;
                }
                if (!string.IsNullOrEmpty(current))
                    strs.Add(current);
                StringBlock[] blocks = new StringBlock[strs.Count];
                i = 0;
                foreach (string v in strs)
                {
                    blocks[i] = new StringBlock(v);
                    blocks[i].CheckType();
                    i++;
                }
                return blocks;
                // return new int[] { };
            }

            public bool CheckMixed()
            {
                CheckType();
                return _type == WordType.Unknown && CheckMixed(_data);
            }
            private static bool IsSplitter(char ch) => TypeMatchingLibrary.Splitters.Contains(ch);
            private static bool CheckMixed(string data)
            {
                HashSet<char> part1 = [' '];
                HashSet<char> part2 = ['+', '-', '*', '/', '%', '=', '<', '>', '='];
                HashSet<char> part3 = ['(', ')'];
                HashSet<char> part4 = ['[', ']'];
                HashSet<char> part5 = ['}', '{'];
                HashSet<char> part6 = ['<', '>'];
                HashSet<char> part7 = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
                HashSet<char> part8 = [];
                for (int i = 0; i < 26; i++)
                { _ = part8.Add((char)('A' + i)); _ = part8.Add((char)('a' + i)); }

                HashSet<char>[] all = [part1, part2, part3, part4, part5, part6, part7, part8];

                bool enabled1 = false;

                foreach (HashSet<char> set in all)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (set.Contains(data[i]))
                        {
                            if (enabled1)
                                return true;
                            enabled1 = true;
                            break;
                        }
                    }
                }

                return false;
            }
            public bool IsMergeAvailable(string r) => !CheckMixed(_data + r);
            public void Merge(StringBlock r)
            {
                _data += r;
                CheckType();
            }
        }
    }
}