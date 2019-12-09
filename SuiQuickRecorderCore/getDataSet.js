let accounts = document.getElementById("ul_tb-outAccount-1").getElementsByTagName("li");
let result = "Id,Name,Alt\n";
for(let i = 0; i < accounts.length; i++) {
    result = result + accounts[i].getAttribute("id") + "," + accounts[i].getAttribute("title") + ",\n";
}
console.log("dataAccounts.csv");
console.log(result);

let categoriesIn = document.getElementById("ls-ul1-income").getElementsByTagName("li");
result = "Id,Name,Alt\n";
for(let i = 0; i < categoriesIn.length; i++) {
    if(categoriesIn[i].getAttribute("id") !== null) {
        result = result + categoriesIn[i].getAttribute("id") + "," + categoriesIn[i].getElementsByTagName("span")[0].getAttribute("title") + ",\n";
    }
}
console.log("dataCategoriesIn.csv");
console.log(result);

let categoriesOut = document.getElementById("ls-ul1-payout").getElementsByTagName("li");
result = "Id,Name,Alt\n";
for(let i = 0; i < categoriesOut.length; i++) {
    if(categoriesOut[i].getAttribute("id") !== null) {
        result = result + categoriesOut[i].getAttribute("id") + "," + categoriesOut[i].getElementsByTagName("span")[0].getAttribute("title") + ",\n";
    }
}
console.log("dataCategoriesOut.csv");
console.log(result);

let stores = document.getElementById("ul_tb-store").getElementsByTagName("li");
result = "Id,Name,Alt\n";
for(let i = 0; i < stores.length; i++) {
    result = result + stores[i].getAttribute("id") + "," + stores[i].getAttribute("title") + ",\n";
}
console.log("dataStores.csv");
console.log(result);