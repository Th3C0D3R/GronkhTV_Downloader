var CScallback = arguments[arguments.length - 1];
async function makeRequest(url) {
    console.log(url);
    var data = await fetch(url);
    try {
        data = await data.text();
        return CScallback(data);
    }
    catch (e) {
        return CScallback(JSON.stringify(e));
    }
};

makeRequest(arguments[0]);