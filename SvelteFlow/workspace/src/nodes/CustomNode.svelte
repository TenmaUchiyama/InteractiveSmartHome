<script>
    export let id; // NodeのID
    export let label; // Nodeのラベル
    export let x = 0; // X座標
    export let y = 0; // Y座標
  </script>
  
  <div class="node" style="left: {x}px; top: {y}px;">
    <h3>{label}</h3>
    <p>ID: {id}</p>
  </div>
  
  <style>
    .node {
      position: absolute;
      background-color: lightblue;
      padding: 10px;
      border-radius: 5px;
      border: 1px solid #000;
      width: 100px;
      text-align: center;
    }
  </style>
  