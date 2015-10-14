using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Git.Server;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Configuration;
using System.Net;

namespace TeamFoundation.VersionControl.PushNotificationSubscriber
{
    public class GitPushWorkItemNotificationSubscriber : ISubscriber
    {
        private static readonly string VALIDATION_PATTERN = @"(^#\d{1,7}($|\s.*)|.*\s#\d{1,7}($|\s.*))";

        public string Name
        {
            get { return "Git Push WorkItem"; }
        }

        public SubscriberPriority Priority
        {
            get { return SubscriberPriority.Normal; }
        }

        public EventNotificationStatus ProcessEvent__TESTE(TeamFoundationRequestContext requestContext,
            NotificationType notificationType, object notificationEventArgs, out int statusCode,
            out string statusMessage, out ExceptionPropertyCollection properties)
        {
            statusCode = 1;
            statusMessage = "É preciso associar uma work item a todos os seus commits! Use # seguido do número da work item no comentário de cada commit.";
            properties = null;

            return EventNotificationStatus.ActionDenied;
        }

        public EventNotificationStatus ProcessEvent(TeamFoundationRequestContext requestContext,
            NotificationType notificationType, object notificationEventArgs, out int statusCode,
            out string statusMessage, out ExceptionPropertyCollection properties)
        {
            if (notificationType == NotificationType.DecisionPoint && notificationEventArgs is PushNotification)
            {
                PushNotification pushNotification = notificationEventArgs as PushNotification;

                //primeiro carrega o serviço do repositório
                TeamFoundationGitRepositoryService repositoryService =
                    requestContext.GetService<TeamFoundationGitRepositoryService>();

                //obter o id do repositório
                using (TfsGitRepository repository =
                    repositoryService.FindRepositoryById(requestContext, pushNotification.RepositoryId))
                {
                    //verifica todos os commits incluídos
                    foreach (var item in pushNotification.IncludedCommits)
                    {
                        //obtem o commit
                        TfsGitCommit gitCommit = (TfsGitCommit)repository.LookupObject(requestContext, item);

                        //mensagem do commit
                        var comment = gitCommit.GetComment(requestContext);

                        //cria o padrao para validacao do comentario
                        Regex regex = new Regex(VALIDATION_PATTERN);
                        Match match = regex.Match(comment);

                        //validação do comentário e wi
                        //if (!match.Success || ObterWorkItem(Int32.Parse(match.Value.Substring(1)), requestContext) == null)
                        if (!match.Success)
                        {
                            statusCode = 1;
                            statusMessage = "É preciso associar uma work item a todos os seus commits! Use # seguido do número da work item no comentário de cada commit.";
                            properties = null;

                            return EventNotificationStatus.ActionDenied;
                        }
                    }
                }
            }

            statusCode = 0;
            statusMessage = string.Empty;
            properties = null;

            return EventNotificationStatus.ActionApproved;
        }

        public Type[] SubscribedTypes()
        {
            return new Type[1] { typeof(PushNotification) };
        }

        /*
        private WorkItemStore GetWorkItemStore(TeamFoundationRequestContext requestContext, string usuario, string senha, string dominio)
        {
            Uri requestUri = new Uri(requestContext.GetService<TeamFoundationLocationService>().GetServerAccessMapping(requestContext).AccessPoint.Replace("localhost", Environment.MachineName) + "/" + requestContext.ServiceHost.Name);

            NetworkCredential nc = new NetworkCredential(usuario, dominio, senha);
            TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(requestUri, nc);
            tpc.EnsureAuthenticated();
            WorkItemStore store = tpc.GetService<WorkItemStore>();

            return store;
        }

        private WorkItem ObterWorkItem(int codigo, TeamFoundationRequestContext requestContext, string usuario, string senha, string dominio)
        {
            WorkItemStore workItemStore = GetWorkItemStore(requestContext, usuario, senha, dominio);

            return workItemStore.GetWorkItem(codigo);
        } */
    }
}
