<template>
  <div id="locationMap" v-bind:style="mapLayout">
  </div>
</template>
<script>
let svgIcon = `<svg id="svg_icon" data-name="sgv icon" xmlns="http://www.w3.org/2000/svg" width="26" height="26" viewBox="0 0 276.8091 258.1755"><defs><style>.cls-1{fill:#c1272d;}</style></defs><ellipse class="cls-1" cx="50.5014" cy="147.1987" rx="37.5474" ry="45.9683" transform="translate(-70.5218 7.946) rotate(-25.9532)"/><ellipse class="cls-1" cx="96.8048" cy="53.5" rx="40" ry="53.5"/><ellipse class="cls-1" cx="178.8048" cy="53.5" rx="40" ry="53.5"/><ellipse class="cls-1" cx="248.8571" cy="146.7143" rx="45.9683" ry="37.5474" transform="translate(0.5821 282.7843) rotate(-65.3697)"/><path class="cls-1" d="M242.5,227.5c3.93,14.0724-8.6725,29.0464-12,33-21.1938,25.1818-57.47,25.995-80,26.5-24.0362.5388-67.0485,1.503-85-25.5-1.4277-2.1476-11.75-17.8794-7-33,3.1221-9.9393,9.4086-8.944,19-26,5.2441-9.3253,9.2406-16.4322,11.0586-25.2523a63.7424,63.7424,0,0,1,4.7585-14.0421A35.86,35.86,0,0,1,97.5,156.5c15.8211-20.5323,47.0461-20.5052,53-20.5,8.4418.0074,33.0312.0287,48,16.5a37.0823,37.0823,0,0,1,5.1189,6.9933,66.0333,66.0333,0,0,1,2.9466,5.992,73.2927,73.2927,0,0,1,3.9076,11.7384c2.438,9.8945,9.4882,21.204,12.0269,25.2763C233.0946,219.4951,239.9314,218.3026,242.5,227.5Z" transform="translate(-11.1952 -29)"/></svg>`;

export default {
  created(){
    window.addEventListener("resize", this.resized);
  },
    data() {
      var html = document.documentElement;
      var cnh =  html.clientHeight*.8;
      return {
          mapLayout: {
            height: (cnh) + "px",
            'margin-bottom':'-20px',
            'min-width': '400px',
            width: '100%',
            "padding-top": '10px'
        }
      }
  },
  computed: {
  },
  methods: {
    resized(e){
      var rs;
      clearTimeout(rs);
      rs = setTimeout(this.updateUI, 250);
    },
    updateUI(){
      var sidebar = document.getElementById('style-3').clientHeight;
      this.mapLayout['height']=(sidebar*.9) + "px";
    }
  },
  mounted() {
      var map, markers=[];
      var locations = [
        {properties: {Name: 'Brooklyn', Phone: '888-555-1212', Address: '123 Main Street'}, locale:[-73.9442,40.6782]},
        {properties: {Name: 'Calgary', Phone: '888-555-1212', Address: '123 Main Street'}, locale:[-114.0719,51.0447]},
        {properties: {Name: 'Clearwater', Phone: '888-555-1212', Address: '123 Main Street'}, locale:[-82.8001026,27.9658533]},
        {properties: {Name: 'Denver', Phone: '888-555-1212', Address: '123 Main Street'}, locale:[-104.9903,39.7392]},
        {properties: {Name: 'Los Angeles', Phone: '888-555-1212', Address: '123 Main Street'}, locale:[-118.2437,34.0522]},
        {properties: {Name: 'Norman', Phone: '888-555-1212', Address: '123 Main Street'}, locale:[-97.4395,35.2226]},
        {properties: {Name: 'Toronto', Phone: '888-555-1212', Address: '123 Main Street'}, locale:[-79.3832,43.6532]},
        {properties: {Name: 'Hampton Roads', Phone: '888-555-1212', Address: '123 Main Street'}, locale:[-76.30064,36.86562]},
        {properties: {Name: 'Dallas', Phone: '888-555-1212', Address: '123 Main Street'}, locale:[-96.7970,32.7767]}
      ];

          function GetMap() {

            map = new atlas.Map('locationMap', {
                center: [-95, 40],
                zoom: 3,
                showLogo: false,
                style: 'night',
                view: 'Auto',
                showFeedbackLink: false,
                renderWorldCopies: false,
                showBuildingModels: false,
                authOptions: {
                    authType: 'subscriptionKey',
                    subscriptionKey: 'yNB_WHf_R23pAsazj4ynKLqYylMDlDyC-FFTTAN0H14'
                }
            });

            map.events.add('ready', function () {

              for(var i=0; i < locations.length; i++){
                let marker = new atlas.HtmlMarker({
                  properties: locations[i].properties,
                  htmlContent: svgIcon,
                  position: locations[i].locale
                });
                
                marker.setOptions({
                  popup: new atlas.Popup({
                    content: getMarkerPopupContent(locations[i]),
                    pixelOffset: [0, -5]
                      })
                });
                
                map.markers.add(marker);
                map.events.add('click', marker, markerClicked);
                markers.push(marker);
                
            };
            });
          }
          
          function markerClicked(e) {
              var m = e.target;
              for (var j = 0; j < markers.length; j++) {
                  markers[j].getOptions().popup.close();
              }
              m.togglePopup();
          }
          
          function getMarkerPopupContent(location) {
              // minimal data for now
              var desc = [`<div class="container"><div class="row"><div class="col-sm"><h4>${location.properties.Name}</h4></div></div>`]
              desc.push(`<div class="row"><div class="col-sm">${location.properties.Address}</div></div>`);
              desc.push(`<div class="row"><div class="col-sm">${location.properties.Phone}</div></div></div>`);
              return desc.join('');
          }
        
          GetMap();

  }
};
</script>
<style>

/* lose the 1 px border when canvas is focused */
.mapboxgl-canvas:focus {
  outline: transparent auto 0px;
}

.popup-content-container {
  max-width: 180px;
  padding: 10px 0px 20px 0px;
  background-color: #222 !important;
  display: block;
  box-shadow: 3px 3px 4px rgb(117 47 39 / 10%);
  min-width: 120px;
  border-radius: 8px;
  white-space: nowrap;
  position: relative;
  top: -20px;
  right: -36px;
  color: #CCC !important
}

.popup-arrow {
  display: block;
  width: 0;
  height: 0;
  border: 10px solid transparent;
  z-index: 1;
}
.popup-container.bottom>.popup-arrow {
  position: relative;
  align-self: center;
  border-bottom: none;
  border-top-color: #222;
  top: -20px;
}

.popup-close {
  position: absolute;
  top: 3px;
  right: 3px;
  color: #555;
  font-size: 20px;
  font-family: Arial,Helvetica,sans-serif;
  line-height: 20px;
  height: 20px;
  width: 20px;
  text-align: center;
  cursor: pointer;
  background: 0 0;
  border: 0;
  padding: 0;
}
</style>
