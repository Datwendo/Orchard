/**
 * plugin.js
 *
 * Released under LGPL License.
 * Copyright (c) 1999-2015 Ephox Corp. All rights reserved
 *
 * License: http://www.tinymce.com/license
 * Contributing: http://www.tinymce.com/contributing
 */

/*global tinymce:true */

tinymce.PluginManager.add('jmedia', function(editor) {
    function openDialog() {
        editor.focus();
        var $ = editor.getWin().parent.jQuery;
        var rooturl = $("#" + editor.id).data("jmedia-url");
        var url = rooturl + "/Admin/Orchard.MediaLibrary?dialog=true";
        $.colorbox({
            href: url,
            iframe: true,
            reposition: true,
            width: "90%",
            height: "90%",
            onLoad: function() {
                // hide the scrollbars from the main window
                $('html, body').css('overflow', 'hidden');
                //$('#cboxClose').remove();
            },
            onClosed: function() {
                $('html, body').css('overflow', '');

                var selectedData = $.colorbox.selectedData;

                if (selectedData == null) {
                    return;
                }

                var newContent = '';
                for (var i = 0; i < selectedData.length; i++) {
                    var renderMedia = rooturl + "/Admin/Orchard.MediaLibrary/MediaItem/" + selectedData[i].id + "?displayType=Raw";
                    $.ajax({
                        async: false,
                        type: 'GET',
                        url: renderMedia,
                        success: function(data) {
                            newContent += data;
                        }
                    });
                }
                // reassign the src to force a refresh
                editor.execCommand('mceReplaceContent', false, newContent);
            }
        });
    }
    editor.addCommand('mceJMedia', openDialog);

    editor.addButton('jmedia', {
        icon: 'image',
        title: 'Insert Media from Jingwei', //ed.getParam("mediapicker_title"),
        cmd: 'mceJMedia'
    });
    editor.addMenuItem('jmedia', {
        icon: 'image',
        text: 'Anchor',
        context: 'insert',
        cmd: 'mceJMedia'
    });
    /*
    function getInfo() {
        return {
            longname: 'Dawtendo Media Library Plugin',
            author: 'C.Surieux',
            authorurl: 'https://datwendo.com',
            infourl: 'https://datwendo.com',
            version: '1.0'
        };
    }*/
});