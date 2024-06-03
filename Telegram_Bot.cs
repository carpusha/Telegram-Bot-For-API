using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;
using Newtonsoft.Json;
using TelegramBot;

namespace ApiGAMESBot
{
    public class Telegram_Bot
    {
        TelegramBotClient botClient = new TelegramBotClient("6948389309:AAEwmioHe8hhXtYAy02stMR-B6eKdSgkbAk");
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };


        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerErrorAsync, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Бот {botMe.Username} працює");
            Console.ReadKey();
        }

        private Task HandlerErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в API телеграм-бота:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await HandlerMessageStartAsync(botClient, update.Message);
            }
        }

        private async Task HandlerMessageStartAsync(ITelegramBotClient botClient, Message message)
        {
            try
            {
                if (message.Text == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть команду зі списку команд /keyboard");
                    return;
                }
                if (message.Text == "/keyboard")
                {
                    ReplyKeyboardMarkup replyKeyboardMarkup = new(
                        new[]
                        {
                            new KeyboardButton[] { "/getgameinfo", "/getsteamlink", "/addfavorite", "/removefavorite", "/getfavorites" }
                        })
                    {
                        ResizeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: replyKeyboardMarkup);
                    return;
                }
                if (message.Text == "/getgameinfo")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Напишіть назву гри у форматі\n?gamename");
                    return;
                }
                if (message.Text == "/getsteamlink")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Напишіть назву гри у форматі\n!gamename");
                    return;
                }
                if (message.Text == "/addfavorite")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Введіть назву гри у форматі\n+gamename");
                    return;
                }
                if (message.Text == "/removefavorite")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Введіть назву гри у форматі\n-removegamename");
                    return;
                }
                if (message.Text == "/getfavorites")
                {
                    await HandleGetFavoritesRequest(message);
                    return;
                }

                if (message.Text.StartsWith("?"))
                {
                    await HandleGetGameInfoRequest(message);
                }
                else if (message.Text.StartsWith("!"))
                {
                    await HandleGetSteamLinkRequest(message);
                }
                else if (message.Text.StartsWith("+"))
                {
                    await HandleAddFavoriteRequest(message);
                }
                else if (message.Text.StartsWith("-"))
                {
                    await HandleRemoveFavoriteRequest(message);
                }
            }
            catch
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Неправильні дані!");
            }
        }

        private async Task HandleGetGameInfoRequest(Message message)
        {
            var gameName = message.Text.Substring(1);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://localhost:7051/Steam/GetInformationByName?name={gameName}";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var game = JsonConvert.DeserializeObject<TelegramBot.Game>(jsonResponse);

                        string responseText = $"Тип: {game.Type}\n" +
                                              $"Назва: {game.Name}\n" +
                                              $"Ціна: {game.Price}\n" +
                                              $"Дата виходу: {game.ReleaseDate}\n" +
                                              $"Посилання на гру: {game.SteamUrl}\n" +
                                              $"Зображення: {game.ImageUrl}";

                        await botClient.SendTextMessageAsync(message.Chat.Id, responseText);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Не вдалося отримати дані гри.");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Виникла помилка: {ex.Message}");
                }
            }
        }

        private async Task HandleGetSteamLinkRequest(Message message)
        {
            var gameName = message.Text.Substring(1);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://localhost:7051/Steam/GetSteamLinkByName?name={gameName}";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string steamLink = await response.Content.ReadAsStringAsync();
                        string responseText = $"Посилання на цю гру: {steamLink}";
                        await botClient.SendTextMessageAsync(message.Chat.Id, responseText);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Не вдалося отримати посилання Steam.");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Виникла помилка: {ex.Message}");
                }
            }
        }


        private async Task HandleAddFavoriteRequest(Message message)
        {
            var gameName = message.Text.Substring(1);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://localhost:7051/Steam/AddFavoriteGame?name={gameName}";
                    HttpResponseMessage response = await client.PostAsync(url, null);

                    if (response.IsSuccessStatusCode)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Гра '{gameName}' додана до обраного.");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Не вдалося додати гру до обраного.");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Сталася помилка: {ex.Message}");
                }
            }
        }


        private async Task HandleRemoveFavoriteRequest(Message message)
        {
            var gameName = message.Text.Substring(1);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://localhost:7051/Steam/RemoveFavoriteGame?name={gameName}";
                    HttpResponseMessage response = await client.DeleteAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Гра '{gameName}' видалена з обраного.");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Не вдалося видалити гру з обраного.");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Сталася помилка: {ex.Message}");
                }
            }
        }


        private async Task HandleGetFavoritesRequest(Message message)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://localhost:7051/Steam/GetFavoriteGames";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var favoriteGames = JsonConvert.DeserializeObject<List<string>>(jsonResponse);

                        string responseText = "Список Обраних:\n" + string.Join("\n", favoriteGames);
                        await botClient.SendTextMessageAsync(message.Chat.Id, responseText);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Не вдалося отримати улюблені ігри.");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Виникла помилка: {ex.Message}");
                }
            }
        }
    }
}
