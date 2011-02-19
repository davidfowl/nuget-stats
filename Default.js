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

    function update(element, value) {
        value = value.toString();
        var currentValue = $.trim($('#' + element).text().replace(/\s/g, ''));

        if (currentValue != value) {
            var items = $('#' + element + ' span');
            var diff = value.length - currentValue.length;
            $.each(currentValue.split(''), function (i, e) {
                var c = value.charAt(diff + i);
                if (c != e) {
                    var el = $(items[i]);
                    animateEl(el, c);
                }
            });
            for (var i = diff - 1; i >= 0; i--) {
                $('#' + element).prepend('<span>' + value.charAt(i) + '</span>');
            }
        }

        function animateEl(el, v) {
            v = v || '';
            el.animate({ top: 0.3 * parseInt(el.parent().height()) }, 350, 'linear', function () {
                $(this).html(v).css({ top: -0.8 * parseInt(el.parent().height()) }).animate({ top: 0 }, 350, 'linear')
            });
        }
    }

    getStats();
});