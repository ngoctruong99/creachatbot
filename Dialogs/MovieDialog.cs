using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using System.Diagnostics;
using Test.Model;

namespace MultiTurnPromptBot.Dialogs
{
    public class MovieDialog : ComponentDialog

    {
        private readonly IStatePropertyAccessor<MovieProfile> _movieProfileAccessor;

        private readonly MovieDbContext _movie;
        public MovieDialog(MovieDbContext movie)
        {
            _movie = movie;
        }
        public MovieDialog(UserState userState) : base(nameof(MovieDialog))   
        {
            _movieProfileAccessor = userState.CreateProperty<MovieProfile>("MovieProfile");
            var waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync,
                NameConfirmStepAsync,
                PhoneNumberStepAsync,
                PhoneNumberConfirmStepAsync,
               
                MovieTypesStepAsync,
                SummaryStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));


            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);

        
         }
        private static async Task<DialogTurnResult> MovieTypesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Xin mời chọn thể loại phim mà mình thích."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Hành Động", "Kinh Dị", "Hài Hước" }),
                });

        }
        private async Task<DialogTurnResult> Test(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Movie"] = ((FoundChoice)stepContext.Result).Value;
            var a = ((FoundChoice)stepContext.Result).Value;
            
                //    Process myProcess = new Process();
                //    myProcess.StartInfo.UseShellExecute = true;
                //    //myProcess.StartInfo.FileName = "https://localhost:44334/Movie";
                //    myProcess.Start();
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Thanks") }, cancellationToken);
            
        }

        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Xin nhập Tên Của Bạn.") }, cancellationToken);
        }
        private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Đây có phải là tên của bạn không?") }, cancellationToken);
        }
        private static async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Xin nhập Số điện thoại của bạn.") }, cancellationToken);
        }
        private async Task<DialogTurnResult> PhoneNumberConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["PhoneNumber"] = (string)stepContext.Result;

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"PhoneNumber: {stepContext.Result}."), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Đây có phải là số điện thoại của bạn không?") }, cancellationToken);
        }

        
         
        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the current profile object from user state.
            var userProfile = await _movieProfileAccessor.GetAsync(stepContext.Context, () => new MovieProfile(), cancellationToken);

            stepContext.Values["Movie"] = ((FoundChoice)stepContext.Result).Value;
            userProfile.MovieType = (string)stepContext.Values["Movie"];
            userProfile.Name = (string)stepContext.Values["name"];
                userProfile.PhoneNumber = (string)stepContext.Values["PhoneNumber"];
            
            //userProfile.MovieType = (string)stepContext.Values["Movie"];
            //userProfile.Picture = (Attachment)stepContext.Values["picture"];

            //var msg = $"Thể Loại Phim bạn chọn là {userProfile.MovieType} và tên bạn là {userProfile.Name}";
            var msg = $"Tên bạn là {userProfile.Name}";

                
                msg += $" và Số điện thọai của bạn {userProfile.PhoneNumber}";
                msg += $" và Thể loại phim bạn thích là {userProfile.MovieType}";
                msg += ".";

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            User user = new User();
            user.UserId = userProfile.Name;
            user.Password = userProfile.PhoneNumber;
            
            

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Cảm ơn thông tin của bạn sẽ được lưu trữ."), cancellationToken);
            

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }


    }
}
