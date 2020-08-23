namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    public class SelectionResultConfiguration
    {
        private static SelectionResultConfiguration _instance;

        public static SelectionResultConfiguration Get()
        {
            return _instance;
        }

        public static void Set(int lineStart, int depth)
        {
            _instance = new SelectionResultConfiguration(lineStart, depth);
        }

        public int LineStart { get; }

        public int Depth { get; }

        private SelectionResultConfiguration(int lineStart, int depth)
        {
            LineStart = lineStart;
            Depth = depth;
        }
    }
}
