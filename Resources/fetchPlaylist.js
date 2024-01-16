var CScallback = arguments[arguments.length - 1];
async function makeRequest(id) {
    var data = await fetch(`https://api.gronkh.tv/v1/video/playlist?episode=${id}`);
    try {
        data = await data.json();
        if (data["playlist_url"]) {
            return CScallback(JSON.stringify(data["playlist_url"]));
        }
        return CScallback(JSON.stringify(data));
    }
    catch (e) {
        return CScallback(JSON.stringify(e));
    }
};

makeRequest(arguments[0]);