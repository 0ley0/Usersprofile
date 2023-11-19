using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;
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
        [Inject] IDialogService _dialogService {get; set;}
        [Inject] ISnackbar Snackbar {get; set;}
      
        
        private EventCallback<bool>ShowEditRoleChanged {get; set;}

        private List<Users> AllUsers = new();
        private List<Programs> ListProgramUsers = new();
        private List<Programs> ASListAllProgramUsers = new();
        private List<Programs> datadup = new();


        private Users User = new();
        private Users CheckUser = new();
        private int TypeEntry = 1;
        private int TypeEnquiry = 2;
        private int TypeReference = 3;
        private int TypeReport = 4;
        private bool ShowEditRole = false;
        private string? _name;
        private int pageon = 1;
        private string searchString = "";
        private int page = 0;
        private int pageSize = 7;
        private int dataCount = 1000;
        private string _refer_on = null;

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
                .OrderBy(o => o.id)
                .Select(c => new Programs()
                {
                    id = c.id,
                    name = c.name
                })
                .ToListAsync();

                 ASListAllProgramUsers = await appDBcontext.programs
                .Select( x => new Programs {id = x.id , name = x.name , program_type_id = x.program_type_id})
                .OrderBy(x => x.id).ToListAsync();

                // datadup = ListProgramUsers.Concat(ASListAllProgramUsers)
                // .ToLookup(p => p.id)
                // .Select(c => c.Aggregate((l1, l2) => new Programs
                // {
                //     id = l2.id - l1.id
                            
                // })).ToList();

           

            return new List<Programs>();
        }

        private async Task SetCheckedToTrue()
        {
           
            foreach (var items in ASListAllProgramUsers)
            {       
                foreach(var itemsiduser in ListProgramUsers)
                {
                    if (items.id == itemsiduser.id){ items.Isactive = true; _refer_on = "1";}
                }       
                
            }
        }
        
        private async Task DeleteRole()
        {   
            try
            {
                var userID = User.id;
                appDBcontext.programs_users.AsNoTracking().Where(x => x.user_id == userID).ExecuteDelete();     
            }
            catch (Exception er)
            {
                JSRuntime.InvokeVoidAsync("console.log", er);
                throw new Exception(er.ToString());
                
            }            
        }

        private async Task UpdateRole()
        {
            using var transaction  = appDBcontext.Database.BeginTransaction();
            var userId = User.id;
            try
            {
                foreach (var item in ASListAllProgramUsers)
                {                
                    if (item.Isactive)
                    {
                        var NewMyRoleUser = new Programs_Users
                        {
                            user_id = userId,
                            program_id = item.id
                        };
                        await DeleteRole();
                        appDBcontext.programs_users.Remove(NewMyRoleUser);
                        appDBcontext.programs_users.Add(NewMyRoleUser);
                        //JSRuntime.InvokeVoidAsync("console.log", NewMyRoleUser);  
                    }
                    else 
                    {
                    
                       if (userId == User.id){await DeleteRole();}
                        
                    }                 
                }
                    await appDBcontext.SaveChangesAsync();
                    appDBcontext.ChangeTracker.Clear(); 
                    transaction.Commit();
                    string message = "Update Success!";
                    Snackbar.Add(message, Severity.Success , config => 
                        {
                            config.CloseAfterNavigation = true;
                        });

            }
            catch (Exception)
            {      
                    string message = "Update not Success!";
                    Snackbar.Add(message, Severity.Error, config => 
                    {
                        config.CloseAfterNavigation = true;
                    });
                    transaction.Rollback();
            }
        }

        private async Task SaveAsync()
        {
            var UserInTableUser = User.id;
            CheckUser = await appDBcontext.users.AsNoTracking().FirstOrDefaultAsync(x => x.id.Equals(UserInTableUser));
            bool? result = await _dialogService.ShowMessageBox
            (
                "Update Confirmation",
                "Updata can not undone!",
                yesText: "Update!" , cancelText: "Cancel"
            );
                if (result ?? false && User.id == CheckUser.id)
                {
                    await UpdateRole();
                    await Task.Delay(300);
                    await Cancel();
                }
                else 
                {
                    string message = "Cancel to Update!";
                    Snackbar.Add(message, Severity.Warning, config => 
                    {
                        config.CloseAfterNavigation = true;
                    });
                }  
                  
        } 

        private async Task GoToEditUser(int Id , string names)
        {
            _name = names;
            await GetUser(Id);
            await ListProgramuser(Id);
            await SetCheckedToTrue();
            // JSRuntime.InvokeVoidAsync("console.log", ASListAllProgramUsers);
            ShowEditRole = true;
        }

        private async Task Cancel()
        {
            StateHasChanged();
            ShowEditRole = false;
            await ShowEditRoleChanged.InvokeAsync(ShowEditRole);
        }
    }
}