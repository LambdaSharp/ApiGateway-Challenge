var AWS = require("aws-sdk");
const s3 = new AWS.S3();
var s3Params = {
  Bucket: "",
  Key: ""
};

exports.handler = async (event, context, callback) => {
  console.log(JSON.stringify(event));
  if (s3Params.Bucket.length < 1) {
    callback(new Error("S3 Bucket missing"));
    return;
  }
  if (s3Params.Key.length < 1) {
    callback(new Error("S3 Key missing"));
    return;
  }
  try {
    let result;
    if (event.httpMethod == "OPTIONS") {
      result = "";
    } else if (event.httpMethod == "GET") {
      result = await getDataFromS3();
    } else if (event.httpMethod == "POST") {
      //result = 'TODO S3 POST';
    } else if (event.httpMethod == "DELETE") {
      //result = "TODO S3 DELETE";
    } else if (event.httpMethod == "PUT") {
      //result = "TODO S3 PUT";
    }
    callback(null, apiGatewayResponse({ body: result }));
  } catch (e) {
    console.log(JSON.stringify(e));
    callback(null, apiGatewayResponse({ body: e, statusCode: 500 }));
  }
};

function getDataFromS3() {
  return new Promise(function(resolve, reject) {
    s3.getObject(s3Params, function(err, data) {
      if (err) return reject(err);
      let objectData = data.Body.toString("utf-8");
      resolve(JSON.parse(objectData));
    });
  });
}

function saveDataToS3(todos) {
  const saveParams = Object.assign({ Body: JSON.stringify(todos) }, s3Params);
  return new Promise(function(resolve, reject) {
    s3.putObject(saveParams, (err, data) => {
      if (err) return reject(err);
      resolve();
    });
  });
}

function apiGatewayResponse({ body, statusCode = 200 }) {
  return {
    statusCode: statusCode,
    headers: {
      "Access-Control-Allow-Origin": "*",
      "Access-Control-Allow-Methods": "GET,DELETE,POST,PUT,OPTIONS",
      "Access-Control-Allow-Headers": "*"
    },
    body: JSON.stringify(body)
  };
}
