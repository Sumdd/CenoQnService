﻿;layui.define(['jquery','form','laydate','table'],function(e){var $=layui.jquery,table=layui.table,form=layui.form,laydate=layui.laydate,defaultName='default';var core={defaultQuery:{state:false,url:null,},options:{tableName:'sys.v1',debug:false,request:{tokenName:false},response:{statusName:'status',statusCode:{success:0,fail:1,warn:2,error:3,logout:1001},msgName:'msg',dataName:'data'},shadeCloseCase:'!complete'},statusName:function(){return core.options.response.statusName},statusCode:function(){return core.options.response.statusCode},msgName:function(){return core.options.response.msgName},dataName:function(){return core.options.response.dataName},page:function(a){var page=true;a&&$("#"+a).hasClass('no-table-page')&&(page=false);return page},queryString:function(e,f){var a=defaultName;e&&(a=e);var formdata=$("form[lay-filter='"+a+"Search']").serializeArray();var data={};$.each(formdata,function(i,v){v.value&&(data[v.name]=v.value)});$.extend(data,f);return JSON.stringify(data)},loadQuery:function(e,n){if(e){var queryObject=JSON.parse(unescape(e));$.each(queryObject,function(k,v){(k,v)&&$("input[name='"+k+"']").val(v)});if(core.defaultQuery.url){core.defaultQuery.state=true;var a=defaultName,page={curr:1},url=core.defaultQuery.url;n&&(a=n);if(!core.page(a+'Table')){page=false}table.reload(a+'Table',{where:{eqli:'=',queryString:core.queryString(a)},page:page,url:url})}}},sp:function(e){var a=defaultName;e&&(a=e);$("#"+a+"Sp").is(":hidden")?$("#"+a+"Sp").slideDown():$("#"+a+"Sp").slideUp()},spHtml:function(e){var a=defaultName,page={curr:1};e&&(a=e);var html=['<div class="layui-inline">','<button class="layui-btn equal" lay-submit lay-filter="'+a+'Table">精确查询</button>',' ','<button class="layui-btn" lay-submit lay-filter="'+a+'Table">模糊查询</button>',' ','<button type="reset" class="layui-btn layui-btn-primary">重置</button>','</div>'];$("#"+a+"Sp form").not('.no-form-search').append(html.join(''));$("#Btn_"+a+"Search").click(function(){core.sp(a)});if(!core.page(a+'Table')){page=false}form.on('submit('+a+'Table)',function(data){var eqli='like';$(data.elem).hasClass('equal')?(eqli='='):(eqli='like');if(!core.defaultQuery.state&&core.defaultQuery.url){var url=core.defaultQuery.url;core.defaultQuery.url=null;table.reload(a+'Table',{where:{eqli:eqli,queryString:core.queryString(a)},page:page,url:url})}else{table.reload(a+'Table',{where:{eqli:eqli,queryString:core.queryString(a)},page:page})}return false});table.on('sort('+a+'Table)',function(obj){if(!core.defaultQuery.state&&core.defaultQuery.url){var url=core.defaultQuery.url;core.defaultQuery.url=null;table.reload(a+'Table',{initSort:{field:obj.field,type:obj.type},where:{field:obj.field,type:obj.type,queryString:core.queryString(a)},page:page,url:url})}else{table.reload(a+'Table',{initSort:{field:obj.field,type:obj.type},where:{field:obj.field,type:obj.type,queryString:core.queryString(a)},page:page})}return false})},requireHtml:function(){$(':required').parent().parent().find('label').css('color','red')},req:function(e){var l,that=e.that,dos=e.dos;delete e.that,e.dos;$.ajax($.extend({type:'post',contentType:'application/x-www-form-urlencoded',dataType:'json',beforeSend:function(){l=top.layer.open({type:3,icon:1,closeBtn:0,shadeClose:0,shade:[0.1,'#000'],time:0,area:'auto',skin:"layui-layer-molv",title:"提示",content:"",move:0,fixed:1}),that&&!that.hasClass("layui-btn-disabled")&&(that.attr('disabled',true),that.addClass("layui-btn-disabled"))},success:function(data){if(data.status*1===0)core.layInfo(data.msg);else core.layWarn(data.msg);data&&data.uuid&&($('input[name=\'requestID\']').val(data.uuid));"function"==typeof dos&&dos(data)},error:function(e,t){core.layErr(data.msg)},complete:function(){top.layer.close(l),that&&that.hasClass("layui-btn-disabled")&&(that.attr('disabled',false),that.removeClass("layui-btn-disabled"))}},e))},refresh:function(e){var a=defaultName+'Table';e&&(a=e);if(core.page(a)){table.reload(a,{page:{curr:1}})}else{table.reload(a)}},loadLogin:function(e){var $ua=$("#ua");$ua&&($ua.text(e))},open:function(e){top.layui.layer.open($.extend({type:2,resize:true,maxmin:true,area:['555px','75%']},e))},popup:function(e){var n=e.success,r=e.skin;return delete e.success,delete e.skin,top.layui.layer.open($.extend({type:1,skin:r||"layui-layer-molv",title:"提示",content:"",area:"260px",shade:!1,shadeClose:!0,closeBtn:1,offset:"rb",anim:6,move:1,fixed:1,time:10*1000},e))},layInfo:function(e){core.popup({content:e})},layWarn:function(e){core.popup({skin:'layui-layer-warn',content:e,time:30*1000})},layErr:function(e){core.popup({skin:'layui-layer-danger',content:e,time:60*1000})},topTips:function(msg,e){if(e=="ok")return core.popup($.extend({type:1,content:msg,anim:-1},null));if(e=='no')return core.popup($.extend({type:1,content:msg},null));if(e&&e.type&&e.type!=4){delete e.type;return core.popup($.extend({content:msg},e))}else{e&&e.type&&delete e.type;top.layui.layer.msg(msg,e)}},topWindow:function(){return top.window},datepicker:function(){$('.datepicker').each(function(){laydate.render({elem:this,type:'date',trigger:'click'})})},datetimepicker:function(){$('.datetimepicker').each(function(){laydate.render({elem:this,type:'datetime',trigger:'click'})})},timepicker:function(){$('.timepicker').each(function(){laydate.render({elem:this,type:'time',trigger:'click'})})}};$(function(){core.spHtml();core.datepicker();core.datetimepicker();core.timepicker();core.requireHtml()});e('core',core)});