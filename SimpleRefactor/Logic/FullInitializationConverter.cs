using SimpleRefactor.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleRefactor.Logic
{
    internal class FullInitializationConverter
    {
        Regex simplyInitializationRegex = new Regex(@"\w+\s\w+\s?=\s?new\s?\w+\(.{0,}\)\s*\{(\s*\w+\s?=\s?\w+\.\w+\,?){1,}\s*\}\;");
        internal bool IsSimplyInitializationBlock(TextViewSelection selection)
        {
            if (string.IsNullOrEmpty(selection.Text))
                return false;
            return simplyInitializationRegex.IsMatch(selection.Text);
        }

        internal string ConvertToFullInitialization(TextViewSelection selection)
        {
            throw new NotImplementedException();
        }
    }
}
