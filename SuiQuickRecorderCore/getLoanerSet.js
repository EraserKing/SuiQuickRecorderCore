let loanerNodes = document.getElementsByClassName("loan-name j-not")
let result = "Id,Name,Alt\n";
for(let i = 0; i < loanerNodes.length; i++) {
    result = result + loanerNodes[i].getAttribute("id") + "," + loanerNodes[i].getElementsByTagName("a")[0].getAttribute("title") + ",\n";
}
console.log("loaner.csv");
console.log(result);