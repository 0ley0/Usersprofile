using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.JSInterop;
using MudBlazor;
using ProLoginprofile.Models;
using ProLoginprofile.Models.Entities;

namespace ProLoginprofile.Pages.Pagecomponents
{
    public partial class UserProfile
    {
        [Inject] AppDBcontext? appDBcontext {get; set;}
        [Inject] IJSRuntime JSRuntime {get; set;}
        [Inject] NavigationManager NavManager { get; set; }
      

        private EventCallback<bool>ShowEditRoleChanged {get; set;}

        private List<Users> AllUsers = new();
        private List<Programs> ListProgramUsers = new();
        private List<Programs> ListAllProgramUsers = new();
        private List<Programs> datadup = new();
       

        private Users User = new();

        private bool ShowEditRole = false;
        private string? namese;
        private int pageon = 1;
        private string searchString = "";
        private int page = 0;
        private int pageSize = 7;
        private int dataCount = 1000;

        private MudTable<Users>? _table;
        
        


        protected override async Task OnInitializedAsync()
        {
            await GetAllUsers(page,pageSize);
        }
        private async Task<List<Users>> GetAllUsers (int p, int ps)
        {
            dataCount = await appDBcontext.users.Select(x => x.id).CountAsync();
            AllUsers = await appDBcontext.users
            .OrderBy(x => x.id)
            .Skip(p)
            .Take(ps)
            .ToListAsync();
            return AllUsers;
        }
        private async Task PageChangAsync(int i)
        {
            pageon = i;
            _table.NavigateTo(i-1);
            await GetAllUsers((i-1)*pageSize,pageSize);
        }
        private async Task Indexs(string searchString)
        {
            var empqery = from x in appDBcontext.users select x;
            if (!String.IsNullOrEmpty(searchString))
            {   
                empqery = empqery.Where(x => x.login.Contains(searchString)
                || x.email.Contains(searchString));
            }
            dataCount = await empqery.AsNoTracking().CountAsync();
            AllUsers = await empqery.AsNoTracking().OrderBy(x => x.id).ToListAsync();
        }
        private async Task<Users> GetUser(int Id)
        {
            User = await appDBcontext.users.FirstOrDefaultAsync(x => x.id.Equals(Id));
            return User;
        }
        
        private async Task<List<Programs>> ListProgramuser(int Id)
        {
                ListProgramUsers = await appDBcontext.programs
                .Where(p => appDBcontext.programs_users
                .Any(x => x.program_id == p.id && x.user_id == Id))
                .Select(c => new Programs()
                {
                    id = c.id,
                    name = c.name
                })
                .ToListAsync();

                 ListAllProgramUsers = await appDBcontext.programs
                .Where(p => appDBcontext.programs_users
                .Any(o => o.program_id == p.id))
                .Select(c => new Programs()
                {
                    id = c.id,
                    name = c.name

                }).ToListAsync();

                datadup = ListProgramUsers.Concat(ListAllProgramUsers)
                .ToLookup(p => p.id)
                .Select(c => c.Aggregate((l1, l2) => new Programs
                {
                    id = l2.id - l1.id
                            
                })).ToList();


            return new List<Programs>();
        }

        private async Task SetCheckedToTrue(int Id)
        {
           var userids = await appDBcontext.users.FirstOrDefaultAsync(x => x.id.Equals(Id));

            foreach (var items in ListAllProgramUsers)
            {       

                foreach(var itemsiduser in ListProgramUsers)
                {
                    if (items.id == itemsiduser.id)
                    {
                        items.Isactive = true;
                    }
                }       
                
            }
            
        }      
        private async Task GoToEditUser(int Id , string names)
        {
            namese = names;
            await GetUser(Id);
            await ListProgramuser(Id);
            await SetCheckedToTrue(Id);
            ShowEditRole = true;
            JSRuntime.InvokeVoidAsync("console.log", SetCheckedToTrue(Id));

        }
        private async Task Cancel()
        {
            //NavManager.NavigateTo("/UserProfile",true);
            // await SetCheckedToTrue();
            // StateHasChanged();
            ShowEditRole = false;
            await ShowEditRoleChanged.InvokeAsync(ShowEditRole);
        }
    }
}