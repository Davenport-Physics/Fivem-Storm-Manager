
using System;
using System.Configuration;
using System.Linq;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace FivemStormManager
{
    static class Git
    {
        private static string git_remote = string.Empty;
        private static string git_repo   = string.Empty;
        private static string git_user   = string.Empty;
        private static string git_pass   = string.Empty;
        private static string git_email  = string.Empty;

        public static void InitGit()
        {
            git_remote = ConfigurationManager.AppSettings["GitRemote"];
            git_repo   = ConfigurationManager.AppSettings["GitRepo"];
            git_user   = ConfigurationManager.AppSettings["StormBotUser"];
            git_pass   = ConfigurationManager.AppSettings["StormBotPass"];
            git_email  = ConfigurationManager.AppSettings["StormBotEmail"];
        }

        public static void PullLatest()
        {
            using (var repo = new Repository(git_repo))
            {
                PullOptions options  = new PullOptions();
                options.FetchOptions = new FetchOptions();
                options.FetchOptions.CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) =>
                new UsernamePasswordCredentials()
                {
                    Username = git_user,
                    Password = git_pass
                });

                Signature signature = new Signature(new Identity(git_user, git_email), DateTimeOffset.Now);
                Commands.Pull(repo, signature, options);
            }
        }

        public static void ResetToLocalHead()
        {
            using (var repo = new Repository(git_repo))
            {
                Commit previousCommit = repo.Head.Commits.ElementAt(1);
                repo.Reset(ResetMode.Hard, previousCommit);
            }
        }
    }
}
