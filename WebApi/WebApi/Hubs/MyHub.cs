using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Hubs
{
    public class MyHub : Hub
    {
        private readonly DataContext context;
        public MyHub(DataContext context)
        {
            this.context = context;
        }
        private static int ClientCount { get; set; } = 0;
        private static List<string> Names { get; set; } = new();
        public static int TeamCount { get; set; } = 7;
        public async Task SendName(string name)
        {
            if (Names.Count >= TeamCount)
            {
                await Clients.Caller.SendAsync("Error", $"Takım en fazla {TeamCount} kişi olabilir");
            }
            else
            {
                Names.Add(name);
                await Clients.All.SendAsync("ReceiveMessage", name);
            }
        }
        public async Task GetNames()
        {
            await Clients.All.SendAsync("ReceiveNames", Names);
        }
        public override async Task OnConnectedAsync()
        {
            ClientCount++;
            await Clients.All.SendAsync("ReceiveClientCount", ClientCount);
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            ClientCount--;
            await Clients.All.SendAsync("ReceiveClientCount", ClientCount);

            await base.OnDisconnectedAsync(exception);
        }
        //Groups
        public async Task SendNameByGroup(string name,string teamName)
        {
            var team = context.Teams.FirstOrDefault(x => x.Name == teamName);
            if (team!=null)
            {
                team.Users.Add(new User {Name = name});
            }
            else
            {
                var newteam = new Team
                {
                    Name = teamName
                };
                newteam.Users.Add(new User(){Name = name});
                await context.Teams.AddAsync(newteam);
            }

            await context.SaveChangesAsync();

            await Clients.Group(teamName).SendAsync("ReceiveMessageByGroup",name,team.Id);

        }

        public async Task GetNamesyGroup()
        {
            var teams = context.Teams.Include(x => x.Users).Select(x => new
            {
                teamName = x.Name,
                Users = x.Users
            }).ToList();
            await Clients.All.SendAsync("ReceiveNamesByGroup", teams);
        }



        public async Task AddGroup(string teamName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, teamName);
        }
        public async Task RemoveToGroup(string teamName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, teamName);
        }

    }
}
