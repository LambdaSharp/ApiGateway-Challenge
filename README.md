# June2018-ApiGateway

In this challenge we're going to learn how to setup an API Gateway, AWS Lambda function, and a datasource to presist data from an existing RESTful application.

<https://skittleson.github.io/ToDoApp/>

![to do app](todoApp.gif)

The architecture should look like this:

![to do app flow](flow.png)


## Pre-requisites

The following tools and accounts are required to complete these instructions.

- [Complete Step 1 of the AWS Lambda Getting Started Guide](http://docs.aws.amazon.com/lambda/latest/dg/setup.html)
  - Setup an AWS account
  - [Setup the AWS CLI](https://docs.aws.amazon.com/lambda/latest/dg/setup-awscli.html)
- Both NodeJS and .NET Core 2+ is required
  - <https://nodejs.org/en/>
  - <https://www.microsoft.com/net/learn/get-started/windows>
  - MySql Workbench if planning to use RDS

## Level 0 - Setup

Pick a data source (AWS's RDS MySql or AWS's DynamoDB). Create a new instance.

<details>
    <summary>RDS MySql</summary>

    This assumes you know how to connect to a standard MySql database using a workbench. **Allocating can take some time.**

    - Select a **Dev/Test** instance.

Next Page

    - DB instance class: db.t2.micro
    - DB instance identifier: toDoDb
    - Master username: dbadmin
    - Master password: *anything you can remember*

Next Page

    - Public accessibility: yes
    - Database name: todo

Launch DB Instance (this will take some time. move to level 1)

Once you have access, use the following sql script to create the table.

```sql
CREATE TABLE todo.items (
  id int(11) NOT NULL AUTO_INCREMENT,
  note text DEFAULT NULL,
  completed bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (id)
)
ENGINE = INNODB
AUTO_INCREMENT = 1
CHARACTER SET latin1
COLLATE latin1_swedish_ci;
```

</details>

<details>
  <summary>DynamoDB</summary>

    Create a new table. Partion by `id` as a number.  

    One item will look like this:

```json
{
    "id": 1,
    "note": "Buy milk",
    "completed": false
}
```

[AWS Reference for DynamoDB NodeJS](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/GettingStarted.NodeJs.03.html)
https://docs.aws.amazon.com/AWSJavaScriptSDK/latest/AWS/DynamoDB.html

</details>

## Level 1 - Lambda function from choosen data source

Create an AWS's Lambda function that will retrieve a list of To Do items in either data store. It can be C#, JavaScript, Python, or any language that is supported.
[AWS RDS Reference](https://docs.aws.amazon.com/lambda/latest/dg/vpc-rds-create-lambda-function.html)
[AWS DynamoDB](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/GettingStarted.NodeJs.03.html)

### Expected Response from Lambda Function on Http Get
```json
[
  {
    "id": 1,
    "note": "Buy milk",
    "completed": false
  },
  {
    "id": 2,
    "note": "Pickup dry cleaning",
    "completed": false
  },
  {
    "id": 3,
    "note": "Pay bills",
    "completed": false
  },
  {
    "id": 4,
    "note": "Schedule tee time",
    "completed": true
}
```

<details>
  <summary>Hints:</summary>

    If using RDS, check the security group of the database configure.

Starter lambda function
```javascript
exports.handler = (event, context, callback) => {
  console.log(JSON.stringify(event));
  try {
    callback(null, buildResponse([]));
  } catch(e){
    callback(null, buildResponse("Failed"));
  }
};

function buildResponse(body){
   return {
        statusCode: 200,
        headers: {
            "Access-Control-Allow-Origin" : "*"
        },
        body: JSON.stringify(body)
    };
}

```

</details>

## Level 2 - Create an API Gateway

- Create an API Gateway.
- Create a new resource `todo`.
- Connect the new resource to the lambda function created from level 1.  Be sure to use `Use Lambda Proxy integration`.
- Deploy API and configure the To Do App. Upon saving, the app will attempt to connect for a list of items. 
![to do app](ConfigureToDoApp.PNG)

[AWS Reference for example of Lambda and API Gateway](https://docs.aws.amazon.com/apigateway/latest/developerguide/api-gateway-create-api-as-simple-proxy-for-lambda.html)

<details>
  <summary>Hints:</summary>

    Be sure to `Deploy` the Api! Action drop down, `Deploy Api` on every change.

   Chrome console will throw an error about if origin is not set (also see level 1 javascript hint): `Access-Control-Allow-Origin`
To understand and resolve this security issue it: https://docs.aws.amazon.com/apigateway/latest/developerguide/how-to-cors.html



Check the CloudWatch for Lambda log events.

</details>

[AWS doc information about API Gateway](https://docs.aws.amazon.com/apigateway/latest/developerguide/welcome.html)


## Level 3 - Create, Delete & Update a To Do Item

The To Do app also supports the following functions:
- On resource action method `Post`, create a to do item to data store.
- On resource action method `Delete` resource, delete a to do item to data store.
- On resource action method `Put` resource, update a to do item's `completed` boolean value to data store.

Add both of these features by adding new resources in the api gateway and update the lambda function (or create new ones to handle logic).

<details>
  <summary>Hints:</summary>

    Be sure to `Deploy` the Api! Action drop down, `Deploy Api` on every change.

    Check the network tab in chrome for the requests!

</details>

## Level 4 - Security
- Host the To Do app code on AWS's S3 service.
- Configure the Api to only allow requests from only that S3 static site using CORS.

[AWS docs about S3 static site hosting]()

[AWS docs about CORS](https://docs.aws.amazon.com/apigateway/latest/developerguide/how-to-cors.html)
