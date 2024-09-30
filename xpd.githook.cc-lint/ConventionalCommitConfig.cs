namespace xpd.githook.cc_lint;

public class ConventionalCommitConfig
{
    public CommitTypes types { get; set; }

    public class CommitTypes
    {
        public object refactor { get; set; }
        public object fix { get; set; }
        public object feat { get; set; }
        public object build { get; set; }
        public object chore { get; set; }
        public object style { get; set; }
        public object test { get; set; }
        public object docs { get; set; }
        public object perf { get; set; }
        public object revert { get; set; }
    }
}