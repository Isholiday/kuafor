using backend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace backend.Controllers;

[Authorize(Roles = "User,Admin")]
[Route("/Dashboard/[controller]")]
public class HairAdvisorController(IConfiguration configuration) : Controller {

    private readonly string _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not found in configuration");

    [HttpGet]
    public IActionResult Index() {
        return View(new HairAdvisorViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Analyze(HairAdvisorViewModel model) {
        try {
            if (model.Photo == null || model.Photo.Length == 0) {
                ModelState.AddModelError(string.Empty, "Please upload a photo.");
                return View(nameof(Index), model);
            }

            string[] allowedTypes = { "image/jpeg", "image/jpg", "image/png" };
            if (!allowedTypes.Contains(model.Photo.ContentType.ToLower())) {
                ModelState.AddModelError(string.Empty, "Please upload a valid image file (jpg, jpeg, or png)");
                return View(nameof(Index), model);
            }

            using var ms = new MemoryStream();
            await model.Photo.CopyToAsync(ms);
            var imageData = Convert.ToBase64String(ms.ToArray());

            ChatClient chatClient = new("gpt-4o", _apiKey);
            ChatCompletion chatCompletion = chatClient.CompleteChat(
            [
                new SystemChatMessage("You are a hair style advisor AI. Analyze the uploaded photo and provide recommendations in this order: First mention their current hair features briefly. Then suggest 2-3 hairstyles that would complement their features, followed by 2-3 suitable hair colors. Write in flowing paragraphs without numbering or bullet points. Do not mention anything about maintenance, availability, or salon services. Keep the tone friendly and concise, focusing only on style and color suggestions."),
                new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart("Based on this photo, what hairstyles and colors would suit me best? Please do not try to identify the person in the photo."),
                    ChatMessageContentPart.CreateImagePart(new Uri($"data:image/jpeg;base64,{imageData}"))
                ),
            ],
            new ChatCompletionOptions() {
                MaxOutputTokenCount = 256,
            }
            );
            model.RecommendationResult = chatCompletion.Content[0].Text;
            return View(nameof(Index), model);
        } catch (UriFormatException) {
            ModelState.AddModelError(string.Empty, "The uploaded image is too large. Please upload a smaller image.");
            return View(nameof(Index), model);
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An error occurred while processing your request");
            return View(nameof(Index), model);
        }
    }
}
