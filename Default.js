$(function () {
    var currentLatest = {};
    function getStats() {
        $.get('stats', function (stats) {
            update('downloads', stats.TotalDownloads);
            update('unique', stats.UniqueCount);
            update('total', stats.TotalCount);
            update('packagesDay', stats.DayPackages);
            update('downloadsHour', stats.HourDownloads);

            updateList($('#recent ul'), stats.LatestPackages);
            updateList($('#topPackages ul'), stats.TopPackages);

            $('#loading').hide();
            $('#main').show();

            setTimeout(getStats, 8000);
        }, 'json');
    }

    function updateList(list, values) {
        var content = [];
        $.each(values, function (i) {
            content.push('<li><a href="' + this.Url + '">' + this.Id + ' (' + this.Version + ')</a><p>' + this.Desc + '</p></li>');
        });
        list.html(content.join(''));
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