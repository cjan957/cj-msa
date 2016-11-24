using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using testBot.DataModels;
using System.Collections;

namespace testBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public object AttachmentLayouts { get; private set; }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message) //if type is message
            {

                Luis.StockLUIS stLuis = await Luis.LUISStockClient.ParseUserInput(activity.Text);

                //Setup State Client
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                //Bot State
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                var userMessage = activity.Text;
                string endOutput = "";
                bool isText = false;




                if (userData.GetProperty<bool>("launched"))
                {
                    if (userData.GetProperty<bool>("transferNeedsConfirmation"))
                    {
                        if (userMessage.ToLower().Equals("y"))
                        {
                            string pleaseWait = "Please wait, processing your request";
                            Activity infoReply = activity.CreateReply(pleaseWait);
                            await connector.Conversations.ReplyToActivityAsync(infoReply);

                            //go ahead
                            double amountToTransfer = userData.GetProperty<double>("transferAmount");
                            List<customerInfo> accountInfo = await AzureManager.AzureManagerInstance.GetTimelines("1");
                            if (userData.GetProperty<string>("transferFrom") == "cheque" && userData.GetProperty<string>("transferTo") == "saving") //cheque->saving
                            {

                                foreach (customerInfo t in accountInfo)
                                {
                                    if (t.acc1Bal < amountToTransfer)
                                    {
                                        endOutput += "Sorry there's insufficient funds in your Cheque account";
                                        isText = true;
                                    }
                                    else
                                    {
                                        await AzureManager.AzureManagerInstance.UpdateTimeline("1", t.acc1Bal - amountToTransfer, "cheque");
                                        await AzureManager.AzureManagerInstance.UpdateTimeline("1", t.acc2Bal + amountToTransfer, "saving");
                                        endOutput += "Transcation is complete";
                                        isText = true;
                                    }
                                }
                            }
                            else if (userData.GetProperty<string>("transferFrom") == "cheque" && userData.GetProperty<string>("transferTo") == "credit") //cheque->credit
                            {
                                foreach (customerInfo t in accountInfo)
                                {
                                    if (t.acc1Bal < amountToTransfer)
                                    {
                                        endOutput += "Sorry there's insufficient funds in your Cheque account";
                                        isText = true;
                                    }
                                    else
                                    {
                                        await AzureManager.AzureManagerInstance.UpdateTimeline("1", t.acc1Bal - amountToTransfer, "cheque");
                                        await AzureManager.AzureManagerInstance.UpdateTimeline("1", t.acc3Bal + amountToTransfer, "credit");
                                        endOutput += "Transcation is complete";
                                        isText = true;
                                    }
                                }

                            }
                            else if (userData.GetProperty<string>("transferFrom") == "saving" && userData.GetProperty<string>("transferTo") == "cheque") //saving->cheque
                            {
                                foreach (customerInfo t in accountInfo)
                                {
                                    if (t.acc2Bal < amountToTransfer)
                                    {
                                        endOutput += "Sorry there's insufficient funds in your Saving account";
                                        isText = true;
                                    }
                                    else
                                    {
                                        await AzureManager.AzureManagerInstance.UpdateTimeline("1", t.acc2Bal - amountToTransfer, "saving");
                                        await AzureManager.AzureManagerInstance.UpdateTimeline("1", t.acc1Bal + amountToTransfer, "cheque");
                                        endOutput += "Transcation is complete";
                                        isText = true;
                                    }
                                }

                            }
                            else if (userData.GetProperty<string>("transferFrom") == "saving" && userData.GetProperty<string>("transferTo") == "credit") //saving->credit
                            {
                                foreach (customerInfo t in accountInfo)
                                {
                                    if (t.acc2Bal < amountToTransfer)
                                    {
                                        endOutput += "Sorry there's insufficient funds in your Saving account";
                                        isText = true;
                                    }
                                    else
                                    {
                                        await AzureManager.AzureManagerInstance.UpdateTimeline("1", t.acc2Bal - amountToTransfer, "saving");
                                        await AzureManager.AzureManagerInstance.UpdateTimeline("1", t.acc3Bal + amountToTransfer, "credit");
                                        endOutput += "Transcation is complete";
                                        isText = true;
                                    }
                                }
                            }
                            else if (userData.GetProperty<string>("transferFrom") == "credit" && userData.GetProperty<string>("transferTo") == "cheque") //credit->cheque
                            {
                                foreach (customerInfo t in accountInfo)
                                {
                                    if (t.acc3Bal < amountToTransfer)
                                    {
                                        endOutput += "Sorry there's insufficient funds in your Credit Card account";
                                        isText = true;
                                    }
                                    else
                                    {
                                        await AzureManager.AzureManagerInstance.UpdateTimeline("1", t.acc3Bal - amountToTransfer, "credit");
                                        await AzureManager.AzureManagerInstance.UpdateTimeline("1", t.acc1Bal + amountToTransfer, "cheque");
                                        endOutput += "Transcation is complete";
                                        isText = true;
                                    }
                                }

                            }
                            else if (userData.GetProperty<string>("transferFrom") == "credit" && userData.GetProperty<string>("transferTo") == "saving") //credit->saving
                            {
                                foreach (customerInfo t in accountInfo)
                                {
                                    if (t.acc3Bal < amountToTransfer)
                                    {
                                        endOutput += "Sorry there's insufficient funds in your Credit Card account";
                                        isText = true;
                                    }
                                    else
                                    {
                                        await AzureManager.AzureManagerInstance.UpdateTimeline("1", t.acc3Bal - amountToTransfer, "credit");
                                        await AzureManager.AzureManagerInstance.UpdateTimeline("1", t.acc2Bal + amountToTransfer, "saving");
                                        endOutput += "Transcation is complete";
                                        isText = true;
                                    }
                                }

                            }

                            userData.SetProperty<bool>("transferNeedsConfirmation", false);
                            isText = true;
                            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        }
                        else
                        {
                            //cancel
                            userData.SetProperty<bool>("transferNeedsConfirmation", false);
                            endOutput += "Transcation has been canceled";
                            isText = true;
                            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        }
                    }

                    else if (stLuis.topScoringIntent.intent == "transfer")
                    {
                        string toAccount = "";
                        string fromAccount = "";
                        string amount = "";

                        bool invalid = false;

                        try
                        {
                            string test = stLuis.entities[2].type;
                        }
                        catch (Exception e)
                        {
                            invalid = true;
                        }

                        if (!invalid)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                if (stLuis.entities[i].type == "accountTypeTo")
                                {
                                    toAccount = stLuis.entities[i].entity;
                                }
                                else if (stLuis.entities[i].type == "accountTypeFrom")
                                {
                                    fromAccount = stLuis.entities[i].entity;
                                }
                                else if (stLuis.entities[i].type == "number")
                                {
                                    amount = stLuis.entities[i].entity;
                                }
                            }

                            /*
                            if ((string.IsNullOrEmpty(toAccount)) || (string.IsNullOrEmpty(fromAccount)) || (string.IsNullOrEmpty(amount)) || !(amount.All(char.IsDigit)))
                            {
                                endOutput += "Please specify the account you are transferring from/to as well as the amount in numerical values only\n\n";
                                isText = true;
                            }
                            */

                            amount = amount.Replace(" ", string.Empty);
                            double amountValue = Convert.ToDouble(amount); //may use try catch

                            if (fromAccount.Contains("cheque"))
                            {
                                if (toAccount.Contains("saving"))
                                {
                                    userData.SetProperty<string>("transferFrom", "cheque");
                                    userData.SetProperty<string>("transferTo", "saving");
                                    userData.SetProperty<double>("transferAmount", amountValue); //convert to double before pls
                                    userData.SetProperty<bool>("transferNeedsConfirmation", true);
                                    endOutput += "Are you sure that you want to transfer $" + amountValue + " from your Cheque to your Saving account? Y/N";
                                    isText = true;
                                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                                }
                                else if (toAccount.Contains("credit"))
                                {
                                    userData.SetProperty<string>("transferFrom", "cheque");
                                    userData.SetProperty<string>("transferTo", "credit");
                                    userData.SetProperty<double>("transferAmount", amountValue); //convert to double before pls
                                    userData.SetProperty<bool>("transferNeedsConfirmation", true);
                                    endOutput += "Are you sure that you want to transfer $" + amountValue + " from your Cheque to your Credit card account? Y/N";
                                    isText = true;
                                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                                }
                            }
                            else if (fromAccount.Contains("saving"))
                            {
                                if (toAccount.Contains("cheque"))
                                {
                                    userData.SetProperty<string>("transferFrom", "saving");
                                    userData.SetProperty<string>("transferTo", "cheque");
                                    userData.SetProperty<double>("transferAmount", amountValue); //convert to double before pls
                                    userData.SetProperty<bool>("transferNeedsConfirmation", true);
                                    endOutput += "Are you sure that you want to transfer $" + amountValue + " from your Saving to your Cheque account? Y/N";
                                    isText = true;

                                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                                }
                                else if (toAccount.Contains("credit"))
                                {
                                    userData.SetProperty<string>("transferFrom", "saving");
                                    userData.SetProperty<string>("transferTo", "credit");
                                    userData.SetProperty<double>("transferAmount", amountValue); //convert to double before pls
                                    userData.SetProperty<bool>("transferNeedsConfirmation", true);
                                    endOutput += "Are you sure that you want to transfer $" + amountValue + " from your Saving to your Credit card account? Y/N";
                                    isText = true;

                                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                                }

                            }
                            else if (fromAccount.Contains("credit"))
                            {
                                if (toAccount.Contains("cheque"))
                                {
                                    userData.SetProperty<string>("transferFrom", "credit");
                                    userData.SetProperty<string>("transferTo", "cheque");
                                    userData.SetProperty<double>("transferAmount", amountValue); //convert to double before pls
                                    userData.SetProperty<bool>("transferNeedsConfirmation", true);
                                    endOutput += "Are you sure that you want to transfer $" + amountValue + " from your Credit Card to your Cheque account? Y/N";
                                    isText = true;

                                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                                }
                                else if (toAccount.Contains("saving"))
                                {
                                    userData.SetProperty<string>("transferFrom", "credit");
                                    userData.SetProperty<string>("transferTo", "saving");
                                    userData.SetProperty<double>("transferAmount", amountValue); //convert to double before pls
                                    userData.SetProperty<bool>("transferNeedsConfirmation", true);
                                    endOutput += "Are you sure that you want to transfer $" + amountValue + " from your Credit Card to your Saving account? Y/N";
                                    isText = true;

                                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                                }

                            }
                        }
                        else
                        {
                            endOutput += "Please specify the account you are transferring from/to as well as the amount in numerical values only\n\n";
                            isText = true;
                        }
                    }


                    else if (stLuis.topScoringIntent.intent == "getRate")
                    {
                        if (stLuis.entities[0].type == "currency")
                        {
                            Exchange.Rootobject exchange = await Exchange.exchangeClient.ParseUserInput("NZD");
                            if (stLuis.entities[0].entity == "usd")
                            {
                                double amount = exchange.rates.USD;
                                endOutput += "1 NZD equals " + Math.Round(amount,2) + " USD";
                            }
                            else if (stLuis.entities[0].entity == "aud")
                            {
                                double amount = exchange.rates.AUD;
                                endOutput += "1 NZD equals " + Math.Round(amount, 2) + " AUD";
                            }
                            else if (stLuis.entities[0].entity == "cny")
                            {
                                double amount = exchange.rates.CNY;
                                endOutput += "1 NZD equals " + Math.Round(amount, 2) + " CNY";
                            }
                            else if (stLuis.entities[0].entity == "gbp")
                            {
                                double amount = exchange.rates.GBP;
                                endOutput += "1 NZD equals " + Math.Round(amount, 2) + " GBP";
                            }
                            else if (stLuis.entities[0].entity == "hkd")
                            {
                                double amount = exchange.rates.HKD;
                                endOutput += "1 NZD equals " + Math.Round(amount, 2) + " HKD";
                            }
                            else if (stLuis.entities[0].entity == "jpy")
                            {
                                double amount = exchange.rates.JPY;
                                endOutput += "1 NZD equals " + Math.Round(amount, 2) + " JPY";
                            }
                            else if (stLuis.entities[0].entity == "krw")
                            {
                                double amount = exchange.rates.KRW;
                                endOutput += "1 NZD equals " + Math.Round(amount, 2) + " KRW";
                            }
                            else if (stLuis.entities[0].entity == "sgd")
                            {
                                double amount = exchange.rates.SGD;
                                endOutput += "1 NZD equals " + Math.Round(amount, 2) + " SGD";
                            }
                            else if (stLuis.entities[0].entity == "thb")
                            {
                                double amount = exchange.rates.THB;
                                endOutput += "1 NZD equals " + Math.Round(amount, 2) + " THB";
                            }
                            else if (stLuis.entities[0].entity == "eur")
                            {
                                double amount = exchange.rates.EUR;
                                endOutput += "1 NZD equals " + Math.Round(amount, 2) + " EUR";
                            }
                            else
                            {
                                endOutput += "Are you trying to obtain an exchange rate? Can you try again with a different query?";
                            }
                            isText = true;
                        }
                        else
                        {
                            endOutput += "Are you trying to obtain an exchange rate? If so, try again with a slighly different phase";
                            isText = true;
                        }

                    }


                    else if (userMessage.ToLower().Contains("account"))
                    {
                        List<customerInfo> accountInfo = await AzureManager.AzureManagerInstance.GetTimelines("1");
                        bool infoFound = false;
                        foreach (customerInfo t in accountInfo)
                        {
                            infoFound = true;
                            Activity replyToConversation = activity.CreateReply("");
                            replyToConversation.Recipient = activity.From;
                            replyToConversation.Type = "message";
                            replyToConversation.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            replyToConversation.Attachments = new List<Attachment>();

                            string[] accountName = new string[3];
                            string[] accountNum = new string[3];
                            double[] accountBalance = new double[3];
                            string[] accountPicture = new String[3];

                            int numberOfAccount = 0;

                            if (t.acc1No != null)
                            {
                                accountName[numberOfAccount] = t.acc1Name;
                                accountNum[numberOfAccount] = t.acc1No;
                                accountBalance[numberOfAccount] = t.acc1Bal;
                                accountPicture[numberOfAccount] = "http://www.fitsnews.com/wp-content/uploads/2012/11/bank-account-numbers.jpg";
                                numberOfAccount++;
                                if (t.acc2No != null)
                                {
                                    accountName[numberOfAccount] = t.acc2Name;
                                    accountNum[numberOfAccount] = t.acc2No;
                                    accountBalance[numberOfAccount] = t.acc2Bal;
                                    accountPicture[numberOfAccount] = "http://gundomoney.com/wp-content/uploads/2016/01/saving-account-interest-rate.jpg";
                                    numberOfAccount++;
                                }
                                if (t.acc3No != null)
                                {
                                    accountName[numberOfAccount] = t.acc3Name;
                                    accountNum[numberOfAccount] = t.acc3No;
                                    accountBalance[numberOfAccount] = t.acc3Bal;
                                    accountPicture[numberOfAccount] = "http://www.psdgraphics.com/file/credit-card-perspective.jpg";
                                    numberOfAccount++;
                                }

                            }

                            for (int i = 0; i < numberOfAccount; i++)
                            {
                                List<CardImage> cardImages = new List<CardImage>();
                                cardImages.Add(new CardImage(url: accountPicture[i]));
                                HeroCard plCard = new HeroCard()
                                {
                                    Title = $" {accountName[i]}",
                                    Subtitle = $"    {accountNum[i]}", //Account Number
                                    Text = $"Available Balance: ${Math.Round(accountBalance[i],2)}",
                                    Images = cardImages,
                                };
                                Attachment plAttachment = plCard.ToAttachment();
                                replyToConversation.Attachments.Add(plAttachment);
                            }

                            await connector.Conversations.SendToConversationAsync(replyToConversation);

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        if (!infoFound)
                        {
                            endOutput += "Hmm, something gone wrong. Could you please try again later.";
                            isText = true;
                        }
                    }

                    else if (userMessage.ToLower().Contains("detail") || userMessage.ToLower().Contains("info"))
                    {
                        List<customerInfo> timelines = await AzureManager.AzureManagerInstance.GetTimelines("1");

                        foreach (customerInfo t in timelines)
                        {
                            endOutput += "#Your Detail# \n\n ##" + t.firstName + " " + t.lastName + " \n\n**Phone Number:** " + t.phoneNum + "\n\n**Address:** " + t.address + "";
                        }
                        isText = true;
                    }
                    else if (userMessage.ToLower().Contains("help"))
                    {
                        endOutput += "Something I can do: \n\n * Show your account info \n\n * Transfer funds between your accounts \n\n * Show your personal details \n\n * Show exchange rates information";
                        isText = true;
                    }
                    else if (userMessage.ToLower().Contains("thank"))
                    {
                        endOutput += "It's my pleasure";
                        isText = true;
                    }
                    else if (userMessage.ToLower().Contains("hello") || userMessage.ToLower().Contains("hi"))
                    {
                        endOutput += ":)";
                        isText = true;
                    }

                    else if (userMessage.ToLower().Contains("contoso"))
                    {
                        Activity replyToConversation = activity.CreateReply("Here's Contoso Bank Info");
                        replyToConversation.Recipient = activity.From;
                        replyToConversation.Type = "message";
                        replyToConversation.Attachments = new List<Attachment>();

                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: "https://cdn2.f-cdn.com/contestentries/699966/18283068/57a8de35ca18f_thumb900.jpg"));

                        List<CardAction> cardButtons = new List<CardAction>();
                        CardAction plButton = new CardAction()
                        {
                            Value = "http://contoso.com",
                            Type = "openUrl",
                            Title = "Contoso Website"
                        };
                        cardButtons.Add(plButton);

                        ThumbnailCard plCard = new ThumbnailCard()
                        {
                            Title = "Contoso",
                            Subtitle = "Call Centre 0800 888 5555",
                            Images = cardImages,
                            Buttons = cardButtons
                        };

                        Attachment plAttachment = plCard.ToAttachment();
                        replyToConversation.Attachments.Add(plAttachment);
                        await connector.Conversations.SendToConversationAsync(replyToConversation);

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }

                    else if(userMessage.ToLower().Contains("forget"))
                    {
                        endOutput = "User data cleared";
                        await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                        isText = true;
                    }

                    else
                    {
                        endOutput += "I'm not sure I understand. Type 'Help' for more information";
                        isText = true;
                    }

                }
                else
                {
                    Activity message = activity.CreateReply("Hello, welcome to Contoso BotBanking! How can I help you today?");
                    await connector.Conversations.ReplyToActivityAsync(message);
                    userData.SetProperty<bool>("launched", true);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                }

                if (isText)
                {
                    Activity infoReply = activity.CreateReply(endOutput);
                    await connector.Conversations.ReplyToActivityAsync(infoReply);
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing

            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }
            return null;
        }
    }
}