/*document.addEventListener('DOMContentLoaded', function() 
{
	console.log("asd");
	chrome.tabs.query({active: true, currentWindow: true}, function(tabs) 
	{
		chrome.tabs.sendMessage(tabs[0].id, {data: "checkAnimePresent"}, function(response) 
		{
			if (response.data == "animeFound")
			{
				document.getElementById("p1").innerHTML = "Anime found";
			}
			else
			{
				document.getElementById("p1").innerHTML = "False";
			}
		});
	});
}, false);*/
