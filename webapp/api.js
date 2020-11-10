$(document).ready(function () {
    $("#temp_plus").click(() => {
        ModifySetpoint(0.5);
    });
    $("#temp_minus").click(() => {
        ModifySetpoint(-0.5);
    });
    $("#heaterOn").slider({    
        // minimum value
        min: 0,
        // maximum value
        max: 720,
        // increment step
        step: 60,
        // the number of digits shown after the decimal.
        precision: 0,
        // 'horizontal' or 'vertical'
        orientation: 'horizontal',
        // initial value
        value: 0,
        // enable range slider
        range: false,
        // selection placement. 
        // 'before', 'after' or 'none'. 
        // in case of a range slider, the selection will be placed between the handles
        selection: 'before',
        // 'show', 'hide', or 'always'
        tooltip: 'show',
        // show two tooltips one for each handler
        tooltip_split: false,
        // lock to ticks
        lock_to_ticks: false,
        // 'round', 'square', 'triangle' or 'custom'
        handle: 'round',      
        // whether or not the slider is initially enabled
        enabled: true,
        // callback
        formatter: function formatter(val) {
            return 'Current: ' + val;
          }
        }).on("slideStop", ()=>{
            ModifySetpoint(0);
        }).on("slide", function(slideEvt) {
            $("#OverrideDuration").text(slideEvt.value)
        });
    $("#reset").click(Reset);
    LoadValues();
  });

function ModifySetpoint(tempVariation)
{
    let temp = parseFloat($("#setpointValue").html());
    temp = temp + tempVariation
    
    var post = {};
    post.setpoint = temp;
    post.hours = parseInt($("#heaterOn").slider("getValue")) / 60;
    if (post.hours == 0)
        post.hours = 4;
    CallAPIverbose("setpoint/add", "POST", post);
}

function Reset()
{
    CallAPIverbose("setpoint/clear", "POST")
}

function LoadValues()
{
    CallAPIverbose("read", "GET")
}

//api methods
// TODO: manage secrets
var apiUri = "";
var apiKey = "";

function CallAPIverbose(method, webmethod, postdata)
{
    //$("#loading").empty();
    $("#connection").addClass("spinner-grow");
    let url = apiUri + method + '?code=' + apiKey;

    console.log(JSON.stringify(postdata));

    $.ajax({
        type: webmethod,
        contentType: "application/json; charset=UTF-8" ,
        url: url,
        data: JSON.stringify(postdata),
        success: function (data) {
            console.log(data.payload);
            var message = JSON.parse(data.payload);
            $("#tempValue").text(message.Temperature);
            $("#humidityValue").text(message.Humidity);
            $("#setpointValue").text(message.CurrentSetpoint);
            if (message.IsHeaterOn)
            {
                $("body").css('background-color', 'orange');
                $("#heaterIO").attr("src", "./icons/flash_on-24px.svg")
            }
            else
            {
                $("body").css('background-color', 'lightgrey');
                $("#heaterIO").attr("src", "./icons/flash_off-24px.svg")
            }

            if (message.OverrideEnd)
            {
                var d = new Date();
                var now = d.getTime();
                var diff = message.OverrideEnd /1000/60;
                
                $("#heaterOn").slider("setValue", diff);
            }
            else
            {
                $("#heaterOn").slider("setValue", 0);
            }

            console.log("Roger!");
            $("#connection").removeClass("spinner-grow").attr("src", "./icons/wifi_on-24px.svg");
        },
        error: function() {
            $("#connection").removeClass("spinner-grow").attr("src", "./icons/wifi_off-24px.svg");
        },
        dataType: "json"
      });      
}