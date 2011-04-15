$(function () {
    var currentLatest = {};
    function getStats() {
        $.post('stats', function (stats) {
            update('downloads', stats.TotalDownloads);
            update('unique', stats.UniqueCount);
            update('total', stats.TotalCount);
            update('packagesDay', stats.DayPackages);
            update('downloadsHour', stats.HourDownloads);

            $('#recent ul').html($.map(stats.LatestPackages, function(e) { 
                return '<li><a href="' + e.Url + '">' + e.Id + ' (' + e.Version + ')</a></li>'}).join(''));
            
            $('#topPackages ul').html($.map(stats.TopPackages, function(e) { 
                return '<li><a href="' + e.Url + '">' + e.Id + '</a> (' + e.DownloadCount + ' downloads)</li>'}).join(''));

            $('#loading').hide();
            $('#main').show();

            setTimeout(getStats, 8000);
        }, 'json');
    }

    getStats();

    function update(element, value) {
        value = value.toString();
        var currentValue = $.trim($('#' + element).text().replace(/\s/g, ''));
        var self = $('#' + element);
        if (currentValue != value) {
            var length = value.length;
            var currLength = currentValue.length;
            var items = self.children('span');

            if (currLength > length) {
                while (items.length > length) {
                    items.first().remove();
                    items = self.children('span');
                }
            }
            else if (currLength < length) {
                for (var i = currLength; i < length; i++) {
                    self.prepend('<span />');
                }
                items = self.children('span');
            }

            $.each(value.split('').reverse(), function (i, e) {
                var c = (i < currLength) ? currentValue.charAt(currLength - i - 1) : '';
                if (c != e) {
                    var el = $(items[length - i - 1]);
                    animateEl(el, e);
                }
            });
        }

        function animateEl(el, v) {
            v = v || '';
            el.animate({ top: 0.3 * parseInt(el.parent().height()) }, 350, 'linear', function () {
                $(this).html(v).css({ top: -0.8 * parseInt(el.parent().height()) }).animate({ top: 0 }, 350, 'linear')
            });
        }
    }
});



