var CScallback = arguments[arguments.length - 1];
async function makeRequest() {
    var data = await fetch("https://api.gronkh.tv/v1/search?first=24&direction=desc&sort=date");
    try {
        data = await data.json();
        if (data["result"]) {
            if (data["result"]["videos"]) {
                if (data["result"]["videos"].length > 0) {
                    return CScallback(JSON.stringify(data["result"]["videos"]));
                }
            }
            return CScallback(JSON.stringify(data["result"]));
        }
        return CScallback(JSON.stringify(data));
    }
    catch (e) {
        return CScallback(JSON.stringify(e));
    }
};

makeRequest();