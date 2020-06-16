var dotnetHelper = null;

function initJqueryUI(componentRef, searchTerms) {
    dotnetHelper = componentRef;

    // Add demo data
    var availableTags = [
        'ActionScript',
        'AppleScript',
        'Asp',
        'BASIC',
        'C',
        'C++',
        'Clojure',
        'COBOL',
        'ColdFusion',
        'Erlang',
        'Fortran',
        'Groovy',
        'Haskell',
        'Java',
        'JavaScript',
        'Lisp',
        'Perl',
        'PHP',
        'Python',
        'Ruby',
        'Scala',
        'Scheme'
        //{ label: 'ActionScript', value: 'ActionScript', anotherValue: 'anothervalue' }
    ];

    // Basic example
    $('#ac-basic-map-search').autocomplete({
        //source: availableTags,
        //source: function (request, response) {
        //    console.log(request, response);
        //    response(availableTags);
        //},
        source: function (request, response) {
            response(searchTerms);
        },
        select: function (event, ui) {
            //console.log(ui);
            return dotnetHelper.invokeMethodAsync('SelectSearchItem', ui)
                .then(data => addMapMarkers(data));
        },
        //search: function (event, ui) {
        //    console.log("search", event, ui);

        //    console.log($('#ac-basic').val());
        //}
        //search: function (event, ui) {
        //    getSearchTerms();
        //}
        search: function () {
            $(this).parent().addClass('ui-autocomplete-processing');
        },
        open: function () {
            $(this).parent().removeClass('ui-autocomplete-processing');
        }
    });

    //availableTags.length = 0;
    //availableTags.push('test123');

    // Remote data
    $('#ac-remote').autocomplete({
        minLength: 2,
        source: '~/global_assets/demo_data/jquery_ui/autocomplete.php',
        search: function () {
            $(this).parent().addClass('ui-autocomplete-processing');
        },
        open: function () {
            $(this).parent().removeClass('ui-autocomplete-processing');
        }
    });

    // Remote data with caching
    var cache = {};
    $('#ac-caching').autocomplete({
        minLength: 2,
        source: function (request, response) {
            var term = request.term;
            if (term in cache) {
                response(cache[term]);
                return;
            }

            $.getJSON('~/global_assets/demo_data/jquery_ui/autocomplete.php', request, function (data, status, xhr) {
                cache[term] = data;
                response(data);
            });
        },
        search: function () {
            $(this).parent().addClass('ui-autocomplete-processing');
        },
        open: function () {
            $(this).parent().removeClass('ui-autocomplete-processing');
        }
    });
}