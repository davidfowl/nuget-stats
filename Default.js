$(function () {
    function getStats() {
        $.get('stats', function (stats) {
            
            

            update('downloads', stats.TotalDownloads);
            update('unique', stats.UniqueCount);
            update('total', stats.TotalCount);

            var recent = $('#recent ul').html('');
            $.each(stats.LatestPackages, function (i) {
                var value = this.Id + " (" + this.Version + ")";
                recent.append('<li><a href="' + this.Url + '"><strong>' + this.Id + ' (' + this.Version + ')</strong></a></li>');
            });

            $('#loading').hide();
            $('#main').show();

            setTimeout(getStats, 5000);
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
            el.animate({ top: parseInt(0.5 * el.parent().height()) }, 350, 'linear', function () {
                $(this).html(v).css({ top: -parseInt(0.5 * el.parent().height()) }).animate({ top: 0 }, 350, 'linear')
            });
        }
    }
    getStats();
});